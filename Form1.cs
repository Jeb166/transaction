using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Data.SqlClient;
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
        Random rand = new Random();  // Create a single Random object here

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
                Thread thread = new Thread(() => TypeAUserThread());
                threads.Add(thread);
                thread.Start();
            }

            for (int i = 0; i < typeBUsers; i++)
            {
                Thread thread = new Thread(() => TypeBUserThread());
                threads.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            CalculateAndPrintAverageDurations();
            Console.WriteLine("Deadlocks occured by Type A Threads: {0}", a_deadlockCount);
            Console.WriteLine("Deadlocks occured by Type A Threads: {0}", b_deadlockCount);
        }

        private void TypeAUserThread()
        {
            DateTime startTime = DateTime.Now;

            for (int i = 0; i < 100; i++)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();


                    SqlTransaction transaction = connection.BeginTransaction(_isolationLevel);

                    try
                    {
                        if (rand.NextDouble() < 0.5)  // Use the same Random object
                        {
                            RunUpdateQuery(connection, transaction, "20110101", "20111231");
                        }
                        if (rand.NextDouble() < 0.5)
                        {
                            RunUpdateQuery(connection, transaction, "20120101", "20121231");
                        }
                        if (rand.NextDouble() < 0.5)
                        {
                            RunUpdateQuery(connection, transaction, "20130101", "20131231");
                        }
                        if (rand.NextDouble() < 0.5)
                        {
                            RunUpdateQuery(connection, transaction, "20140101", "20141231");
                        }
                        if (rand.NextDouble() < 0.5)
                        {
                            RunUpdateQuery(connection, transaction, "20150101", "20151231");
                        }

                        transaction.Commit();

                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Interlocked.Increment(ref a_deadlockCount);
                        // Handle error
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }

            TimeSpan elapsedTime = DateTime.Now - startTime;
            lock (_lockObject)
            {
                _totalDurationTypeA += elapsedTime.TotalMilliseconds;
                _typeACount++;
            }
        }


        private void TypeBUserThread()
        {
            DateTime startTime = DateTime.Now;

            for (int i = 0; i < 100; i++)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();


                    SqlTransaction transaction = connection.BeginTransaction(_isolationLevel);

                    try
                    {
                        if (rand.NextDouble() < 0.5)
                        {
                            RunSelectQuery(connection, transaction, "20110101", "20111231");
                        }
                        if (rand.NextDouble() < 0.5)
                        {
                            RunSelectQuery(connection, transaction, "20120101", "20121231");
                        }
                        if (rand.NextDouble() < 0.5)
                        {
                            RunSelectQuery(connection, transaction, "20130101", "20131231");
                        }
                        if (rand.NextDouble() < 0.5)
                        {
                            RunSelectQuery(connection, transaction, "20140101", "20141231");
                        }
                        if (rand.NextDouble() < 0.5)
                        {
                            RunSelectQuery(connection, transaction, "20150101", "20151231");
                        }

                        transaction.Commit();


                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        Interlocked.Increment(ref b_deadlockCount);
                        // Handle error
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }


            TimeSpan elapsedTime = DateTime.Now - startTime;
            lock (_lockObject)
            {
                _totalDurationTypeB += elapsedTime.TotalMilliseconds;
                _typeBCount++;
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

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 1205) // Deadlock
                    {
                        transaction.Rollback();
                        Interlocked.Increment(ref a_deadlockCount); // Increment global deadlock count
                        RunUpdateQuery(connection, transaction, beginDate, endDate);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private void RunSelectQuery(SqlConnection connection, SqlTransaction transaction, string beginDate, string endDate)
        {
            using (SqlCommand command = new SqlCommand("SELECT SUM(Sales.SalesOrderDetail.OrderQty) FROM Sales.SalesOrderDetail WHERE UnitPrice > 100 AND EXISTS (SELECT * FROM Sales.SalesOrderHeader WHERE Sales.SalesOrderHeader.SalesOrderID = Sales.SalesOrderDetail.SalesOrderID AND Sales.SalesOrderHeader.OrderDate BETWEEN @BeginDate AND @EndDate AND Sales.SalesOrderHeader.OnlineOrderFlag = 1)", connection, transaction))
            {
                command.Parameters.AddWithValue("@BeginDate", beginDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                try
                {
                    command.ExecuteScalar();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 1205) // Deadlock
                    {
                        transaction.Rollback();
                        Interlocked.Increment(ref b_deadlockCount); // Increment global deadlock count
                        RunSelectQuery(connection, transaction, beginDate, endDate);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}