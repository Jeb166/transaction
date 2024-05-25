using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace transaction
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private ConcurrentDictionary<string, ThreadInfo> simulationResult = new ConcurrentDictionary<string, ThreadInfo>();

        public Form1()
        {
            InitializeComponent();
        }

        private void StartSimulation_Button(object sender, EventArgs e)
        {
            simulationResult.Clear();

            IsolationLevel selectedIsolationLevel = (IsolationLevel)Enum.Parse(typeof(IsolationLevel), comboBoxIsolationLevel.SelectedItem.ToString());
            int countTypeA = (int)nudTypeAUsers.Value;
            int countTypeB = (int)nudTypeBUsers.Value;

            string connectionString = "Server=G513;Database=AdventureWorks2022;Trusted_Connection=True;TrustServerCertificate=true;";

            HandleIndexes(connectionString);

            int totalOperations = (countTypeA + countTypeB) * 100; 

            var threads = new List<Thread>();
            StartThreads(countTypeA, "TypeA", selectedIsolationLevel, connectionString, totalOperations, threads);
            StartThreads(countTypeB, "TypeB", selectedIsolationLevel, connectionString, totalOperations, threads);

            foreach (Thread thread in threads) 
            {
                thread.Join();
            }



            Console.WriteLine($"\n_________________________________________________________");
            Console.WriteLine($"\n{comboBoxIndex.SelectedItem.ToString()}");
            Console.WriteLine($"Isolation Level: {comboBoxIsolationLevel.SelectedItem.ToString()}\n");

            foreach (var key in simulationResult.Keys)
            {
                var stats = simulationResult[key];
                string limitedString = key.Substring(0, 5);

                Console.WriteLine($"User: {limitedString}");
                Console.WriteLine($"Deadlocks: {stats.TotalDeadlocks}");
                Console.WriteLine($"Total Duration: {stats.TotalDuration}");
                Console.WriteLine($"Average Duration: {stats.AverageDuration}");
                Console.WriteLine($"Total Runs: {stats.TotalRuns}");
                Console.WriteLine($"Total Timeout Duration: {stats.TotalTimeoutDuration}\n");
            }
        }

        public class ThreadInfo
        {
            public int TotalRuns { get; set; }
            public long TotalDuration { get; set; }
            public int TotalDeadlocks { get; set; }
            public double AverageDuration { get; set; }
            public long TotalTimeoutDuration { get; set; }
        }

        private void HandleIndexes(string connectionString)
        {
            List<(string TableName1, string IndexName1)> indexes1 = new List<(string, string)>
            {
            ("Sales.SalesOrderDetail", "AK_SalesOrderDetail_rowguid"),
            ("Sales.SalesOrderDetail", "IX_SalesOrderDetail_ProductID"),
            ("Sales.SalesOrderHeader", "AK_SalesOrderHeader_rowguid"),
            ("Sales.SalesOrderHeader", "AK_SalesOrderHeader_SalesOrderNumber"),
            ("Sales.SalesOrderHeader", "IX_SalesOrderHeader_CustomerID"),
            ("Sales.SalesOrderHeader", "IX_SalesOrderHeader_SalesPersonID")
            };

            List<(string TableName2, string IndexName2)> indexes2 = new List<(string, string)>
            {
            ("Sales.SalesOrderDetail", "IX_SalesOrderDetail_UnitPrice_SalesOrderID"),
            ("Sales.SalesOrderHeader", "IX_SalesOrderHeader_SalesOrderID_OrderDate_OnlineOrderFlag")
            };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var (tableName1, indexName1) in indexes1)
                {
                    bool indexExists = IndexExists(connectionString, indexName1, tableName1);

                    if (indexExists)
                    {
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = connection;
                            command.CommandText = $@"
                            DROP INDEX {indexName1} ON {tableName1}";
                            command.ExecuteNonQuery();
                        }
                    }
                }

                string selectedPart = comboBoxIndex.SelectedItem.ToString();

                if (selectedPart == "Part 1: Without Indexes")
                {
                    foreach (var (tableName2, indexName2) in indexes2)
                    {
                        bool indexExists = IndexExists(connectionString, indexName2, tableName2);

                        if (indexExists)
                        {
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = connection;
                                command.CommandText = $@"
                            DROP INDEX {indexName2} ON {tableName2}";
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
                else if (selectedPart == "Part 2: With Indexes")
                {
                    foreach (var (tableName2, indexName2) in indexes2)
                    {
                        bool indexExists = IndexExists(connectionString, indexName2, tableName2);

                        if (!indexExists)
                        {
                            using (SqlCommand command = new SqlCommand())
                            {
                                command.Connection = connection;
                                command.CommandText = $@"
                        -- Index on SalesOrderDetail table for UnitPrice and SalesOrderID
                        CREATE INDEX IX_SalesOrderDetail_UnitPrice_SalesOrderID
                        ON Sales.SalesOrderDetail (UnitPrice, SalesOrderID);

                        -- Index on SalesOrderHeader table for SalesOrderID, OrderDate, and OnlineOrderFlag
                        CREATE INDEX IX_SalesOrderHeader_SalesOrderID_OrderDate_OnlineOrderFlag
                        ON Sales.SalesOrderHeader (SalesOrderID, OrderDate, OnlineOrderFlag);";
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }

                connection.Close();
            }
        }

        bool IndexExists(string connectionString, string indexName, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                SELECT COUNT(*) AS IndexCount
                FROM sys.indexes
                WHERE name = @IndexName
                AND object_id = OBJECT_ID(@TableName)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IndexName", indexName);
                    command.Parameters.AddWithValue("@TableName", tableName);
                    int indexCount = (int)command.ExecuteScalar();
                    return indexCount > 0;
                }
            }
        }

        private void ExecuteQueries(string type, int threadNumber, IsolationLevel isolationLevel, string connectionString, int totalOperations)
        {
            Debug.WriteLine($"{type} operation started for Thread Number {threadNumber} with {isolationLevel} isolation level on {connectionString}.");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int deadlocks = 0;
            long totalTimeoutDuration = 0;
            int timeouts = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to open connection: {ex.Message}");
                    return;
                }

                for (int i = 0; i < 100; i++)
                {
                    Debug.WriteLine($"{type} / Thread number: {threadNumber} / Operation {i} started.");
                    using (SqlCommand command = new SqlCommand())
                    {
                        SqlTransaction transaction = null;
                        try
                        {
                            transaction = connection.BeginTransaction(isolationLevel);
                            command.Connection = connection;
                            command.Transaction = transaction;

                            if (type == "TypeA")
                            {
                                ExecuteUpdateQuery(command);
                            }
                            else if (type == "TypeB")
                            {
                                ExecuteSelectQuery(command);
                            }

                            transaction.Commit();
                        }
                        catch (SqlException ex) when (ex.Number == 1205)
                        {
                            deadlocks++;
                            RollbackTransaction(transaction);
                            Debug.WriteLine("Deadlock encountered. Continuing gracefully.");
                        }
                        catch (SqlException ex) when (ex.Number == -2)
                        {
                            timeouts++;
                            i--;
                            totalTimeoutDuration += 5000; 
                            RollbackTransaction(transaction);
                            Debug.WriteLine("Timeout encountered. Continuing gracefully.");
                        }
                        catch (Exception ex)
                        {
                            RollbackTransaction(transaction);
                            Debug.WriteLine($"An exception occurred: {ex.Message}");
                        }
                    }

                    Debug.WriteLine($"{type} / Thread number: {threadNumber} / Operation {i} executed.");
                }
            }

            stopwatch.Stop();

            long adjustedTotalDuration = stopwatch.ElapsedMilliseconds - totalTimeoutDuration;

            string key = $"{type}_{isolationLevel}_{connectionString}";
            Debug.WriteLine($"Updating simulationResult for user: {key}");

            simulationResult.AddOrUpdate(key,
                new ThreadInfo { TotalRuns = 1, TotalDuration = adjustedTotalDuration, TotalDeadlocks = deadlocks, TotalTimeoutDuration = totalTimeoutDuration },
                (existingKey, existingVal) => new ThreadInfo
                {
                    TotalRuns = existingVal.TotalRuns + 1,
                    TotalDuration = existingVal.TotalDuration + adjustedTotalDuration,
                    TotalDeadlocks = existingVal.TotalDeadlocks + deadlocks,
                    TotalTimeoutDuration = existingVal.TotalTimeoutDuration + totalTimeoutDuration,
                    AverageDuration = (existingVal.TotalDuration + adjustedTotalDuration) / (existingVal.TotalRuns + 1)
                });

            Debug.WriteLine($"{type} operation completed with {isolationLevel} isolation level on {connectionString}. Total duration: {adjustedTotalDuration} ms. Total timeouts: {timeouts}. Total timeout duration: {totalTimeoutDuration} ms.");
        }

        private void StartThreads(int userCount, string userType, IsolationLevel isolationLevel, string connectionString, int totalOperations, List<Thread> threads)
        {
            for (int i = 0; i < userCount; i++)
            {
                int threadIndex = i;
                Thread thread = new Thread(() => ExecuteQueries(userType, threadIndex, isolationLevel, connectionString, totalOperations));
                threads.Add(thread);
                thread.Start();
            }
        }

        private void ExecuteUpdateQuery(SqlCommand command)
        {
            string[] dates = { "20110101", "20120101", "20130101", "20140101", "20150101" };
            foreach (string date in dates)
            {
                if (new Random().NextDouble() < 0.5)
                {
                    command.CommandText = $@"
            UPDATE Sales.SalesOrderDetail
            SET UnitPrice = UnitPrice * 10.0 / 10.0
            WHERE UnitPrice > 100
            AND EXISTS (
                SELECT * FROM Sales.SalesOrderHeader
                WHERE Sales.SalesOrderHeader.SalesOrderID = Sales.SalesOrderDetail.SalesOrderID
                AND Sales.SalesOrderHeader.OrderDate BETWEEN '{date}' AND '{date.Substring(0, 4)}1231'
                AND Sales.SalesOrderHeader.OnlineOrderFlag = 1
            )";
                    command.CommandTimeout = 5;

                    command.ExecuteNonQuery();
                }
            }
        }

        private void ExecuteSelectQuery(SqlCommand command)
        {
            string[] dates = { "20110101", "20120101", "20130101", "20140101", "20150101" };
            foreach (string date in dates)
            {
                if (new Random().NextDouble() < 0.5)
                {
                    command.CommandText = $@"
            SELECT SUM(Sales.SalesOrderDetail.OrderQty)
            FROM Sales.SalesOrderDetail
            WHERE UnitPrice > 100
            AND EXISTS (
                SELECT * FROM Sales.SalesOrderHeader
                WHERE Sales.SalesOrderHeader.SalesOrderID = Sales.SalesOrderDetail.SalesOrderID
                AND Sales.SalesOrderHeader.OrderDate BETWEEN '{date}' AND '{date.Substring(0, 4)}1231'
                AND Sales.SalesOrderHeader.OnlineOrderFlag = 1
            )";
                    command.CommandTimeout = 5;
                    command.ExecuteScalar();
                }
            }
        }

        private void RollbackTransaction(SqlTransaction transaction)
        {
            if (transaction != null)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (InvalidOperationException ex)
                {
                    Debug.WriteLine($"Rollback failed: {ex.Message}");
                }
            }
        }
    }
}
