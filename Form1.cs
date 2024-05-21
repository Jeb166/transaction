using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Windows.Forms;

namespace transaction
{
    public partial class Form1 : Form
    {
        private string _connectionString = "Server=G513;Database=AdventureWorks2019;Trusted_Connection=True;TrustServerCertificate=true;";
        private IsolationLevel _isolationLevel;
        private int a_deadlockCount = 0;
        private int b_deadlockCount = 0;
        private double _totalDurationTypeA = 0;
        private double _totalDurationTypeB = 0;
        private int _typeACount = 0;
        private int _typeBCount = 0;
        private object _lockObject = new object();
        private Random rand = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonStartSimulation_Click(object sender, EventArgs e)
        {
            _isolationLevel = (IsolationLevel)Enum.Parse(typeof(IsolationLevel), comboBoxIsolationLevel.SelectedItem.ToString());

            int typeAUsers = Convert.ToInt32(nudTypeAUsers.Value);
            int typeBUsers = Convert.ToInt32(nudTypeBUsers.Value);

            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < typeAUsers; i++)
            {
                Thread thread = new Thread(TypeAUserThread);
                threads.Add(thread);
                thread.Start();
            }

            for (int i = 0; i < typeBUsers; i++)
            {
                Thread thread = new Thread(TypeBUserThread);
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            CalculateAndPrintAverageDurations();
            Console.WriteLine("Deadlocks occurred by Type A Threads: {0}", a_deadlockCount);
            Console.WriteLine("Deadlocks occurred by Type B Threads: {0}", b_deadlockCount);
        }

        private void TypeAUserThread()
        {
            ExecuteUserThread(
                "A",
                (connection, transaction) =>
                {
                    if (rand.NextDouble() < 0.5) RunUpdateQuery(connection, transaction, "20110101", "20111231");
                    if (rand.NextDouble() < 0.5) RunUpdateQuery(connection, transaction, "20120101", "20121231");
                    if (rand.NextDouble() < 0.5) RunUpdateQuery(connection, transaction, "20130101", "20131231");
                    if (rand.NextDouble() < 0.5) RunUpdateQuery(connection, transaction, "20140101", "20141231");
                    if (rand.NextDouble() < 0.5) RunUpdateQuery(connection, transaction, "20150101", "20151231");
                },
                ref _totalDurationTypeA, ref _typeACount, ref a_deadlockCount);
        }

        private void TypeBUserThread()
        {
            ExecuteUserThread(
                "B",
                (connection, transaction) =>
                {
                    if (rand.NextDouble() < 0.5) RunSelectQuery(connection, transaction, "20110101", "20111231");
                    if (rand.NextDouble() < 0.5) RunSelectQuery(connection, transaction, "20120101", "20121231");
                    if (rand.NextDouble() < 0.5) RunSelectQuery(connection, transaction, "20130101", "20131231");
                    if (rand.NextDouble() < 0.5) RunSelectQuery(connection, transaction, "20140101", "20141231");
                    if (rand.NextDouble() < 0.5) RunSelectQuery(connection, transaction, "20150101", "20151231");
                },
                ref _totalDurationTypeB, ref _typeBCount, ref b_deadlockCount);
        }

        private void ExecuteUserThread(string threadType, Action<SqlConnection, SqlTransaction> action, ref double totalDuration, ref int count, ref int deadlockCount)
        {
            DateTime startTime = DateTime.Now;

            for (int i = 0; i < 100; i++) // Set to 100 iterations
            {
                bool success = false;
                while (!success)
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction(_isolationLevel);

                        try
                        {
                            action(connection, transaction);
                            transaction.Commit();
                            success = true;
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Number == 1205)
                            {
                                Interlocked.Increment(ref deadlockCount);
                                Console.WriteLine($"Deadlock occurred in {threadType} method.");
                            }
                            else if (ex.Number == -2) // SQL Server timeout
                            {
                                Console.WriteLine($"Timeout occurred in {threadType} method: {ex.Message}");
                            }
                            else
                            {
                                Console.WriteLine($"Exception occurred in {threadType} method: {ex.Message}");
                                throw;
                            }
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
            }

            TimeSpan elapsedTime = DateTime.Now - startTime;
            lock (_lockObject)
            {
                totalDuration += elapsedTime.TotalMilliseconds;
                count++;
            }
        }

        private void CalculateAndPrintAverageDurations()
        {
            double averageDurationTypeA = _totalDurationTypeA / _typeACount;
            double averageDurationTypeB = _totalDurationTypeB / _typeBCount;

            Console.WriteLine("Average Duration of Type A Threads: {0} ms", averageDurationTypeA);
            Console.WriteLine("Average Duration of Type B Threads: {0} ms", averageDurationTypeB);
        }

        private void RunUpdateQuery(SqlConnection connection, SqlTransaction transaction, string beginDate, string endDate)
        {
            using (SqlCommand command = new SqlCommand("UPDATE Sales.SalesOrderDetail SET UnitPrice = UnitPrice * 10.0 / 10.0 WHERE UnitPrice > 100 AND EXISTS (SELECT * FROM Sales.SalesOrderHeader WHERE Sales.SalesOrderHeader.SalesOrderID = Sales.SalesOrderDetail.SalesOrderID AND Sales.SalesOrderHeader.OrderDate BETWEEN @BeginDate AND @EndDate AND Sales.SalesOrderHeader.OnlineOrderFlag = 1)", connection, transaction))
            {
                command.Parameters.AddWithValue("@BeginDate", beginDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                command.CommandTimeout = 60; // Increase timeout to 60 seconds
                command.ExecuteNonQuery();
                Console.WriteLine("Update query executed.");
            }
        }

        private void RunSelectQuery(SqlConnection connection, SqlTransaction transaction, string beginDate, string endDate)
        {
            using (SqlCommand command = new SqlCommand("SELECT SUM(Sales.SalesOrderDetail.OrderQty) FROM Sales.SalesOrderDetail WITH (XLOCK, ROWLOCK) WHERE UnitPrice > 100 AND EXISTS (SELECT * FROM Sales.SalesOrderHeader WHERE Sales.SalesOrderHeader.SalesOrderID = Sales.SalesOrderDetail.SalesOrderID AND Sales.SalesOrderHeader.OrderDate BETWEEN @BeginDate AND @EndDate AND Sales.SalesOrderHeader.OnlineOrderFlag = 1)", connection, transaction))
            {
                command.Parameters.AddWithValue("@BeginDate", beginDate);
                command.Parameters.AddWithValue("@EndDate", endDate);
                command.CommandTimeout = 60; // Increase timeout to 60 seconds
                command.ExecuteScalar();
                Console.WriteLine("Select query executed.");
            }
        }
    }
}
