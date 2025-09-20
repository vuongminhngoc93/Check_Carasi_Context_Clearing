using System.IO;
using System.Data.OleDb;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Check_carasi_DF_ContextClearing
{
    class Lib_OLEDB_Excel : IDisposable
    {
        /*Author: Vuong Minh Ngoc (MS/EJV)
        Version: 1.0.0
        Description: transfer data from Excel to OLEDB*/

        // CONNECTION POOLING: Enhanced pool with size limit and auto cleanup
        private static readonly ConcurrentDictionary<string, OleDbConnection> ConnectionPool = 
            new ConcurrentDictionary<string, OleDbConnection>();
        private static readonly ConcurrentDictionary<string, DateTime> LastUsedTime = 
            new ConcurrentDictionary<string, DateTime>();
        private static readonly object PoolLock = new object();
        private static System.Threading.Timer CleanupTimer;
        
        // CONNECTION POOLING: Pool configuration
        private const int MAX_POOL_SIZE = 10; // Limit pool size to prevent overflow
        private static readonly TimeSpan IDLE_TIMEOUT = TimeSpan.FromMinutes(5); // Close idle connections
        
        // CONNECTION POOLING: Initialize cleanup timer
        static Lib_OLEDB_Excel()
        {
            CleanupTimer = new System.Threading.Timer(CleanupIdleConnections, null, 
                TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        private string excelObject = "Provider=Microsoft.{0}.OLEDB.{1};Data Source={2}; Extended Properties =\"Excel {3};HDR=YES\"";
        private string filepath = string.Empty;
        private OleDbConnection con = null;

        /********************** Using to check status of Reading ! :) Use or not is up to you! ******************/
        public delegate void ProgressWork(float percentage);
        private event ProgressWork Reading;
        private event ProgressWork Writeing;
        private event EventHandler connectionStringChange;

        public event ProgressWork ReadProgress
        {
            add
            {
                Reading += value;
            }
            remove
            {
                Reading -= value;
            }
        }

        public virtual void onReadProgress(float percentage)
        {
            if (Reading != null)
                Reading(percentage);
        }

        public event ProgressWork WriteProgress
        {
            add { Writeing += value; }
            remove { Writeing -= value; }
        }

        public virtual void onWriteProgress(float percentage)
        {
            if (Writeing != null)
                Writeing(percentage);
        }

        public event EventHandler ConnectionStringChanged
        {
            add { connectionStringChange += value; }
            remove { connectionStringChange -= value; }
        }
        /*****************************************************************************************************/

        public Lib_OLEDB_Excel(string path)
        {
            this.filepath = path;
            this.onConnectionStringChanged();
        }

        public virtual void onConnectionStringChanged()
        {
            if (this.Connection != null &&
                !this.Connection.ConnectionString.Equals(this.ConnectionString))
            {
                if (this.Connection.State == ConnectionState.Open)
                    this.Connection.Close();
                this.Connection.Dispose();
                this.con = null;
            }
            if (connectionStringChange != null)
            {
                connectionStringChange(this, new EventArgs());
            }
        }

        //ConnectionString
        public string ConnectionString
        {
            get
            {
                if (!(this.filepath == string.Empty))
                {
                    //Check for File Format
                    FileInfo fi = new FileInfo(this.filepath);
                    if (fi.Extension.Equals(".xls"))
                    {
                        // For Excel Below 2007 Format - Use ACE provider for compatibility
                        return string.Format(this.excelObject,
                                   "ACE", "12.0", this.filepath, "8.0");
                    }
                    //xlsx is new format // tmp is temp file which using for only template Excel file which embeded in Resource
                    else if (fi.Extension.Equals(".xlsx")|| fi.Extension.Equals(".tmp"))
                    {
                        // For Excel 2007 File Format
                        return string.Format(this.excelObject,
                                   "ACE", "12.0", this.filepath, "12.0");
                    }
                    else
                        return string.Empty;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        //OleDbConnection to the current File
        public OleDbConnection Connection
        {
            get
            {
                if (con == null)
                {
                    // CONNECTION POOLING: Try to get from pool first
                    string connectionKey = this.ConnectionString;
                    con = GetPooledConnection(connectionKey);
                }
                return this.con;
            }
        }

        // CONNECTION POOLING: Get or create pooled connection
        private OleDbConnection GetPooledConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return null;

            // Try to get existing connection from pool
            if (ConnectionPool.TryGetValue(connectionString, out OleDbConnection pooledConnection))
            {
                // Validate connection is still usable
                if (pooledConnection != null && 
                    (pooledConnection.State == ConnectionState.Open || pooledConnection.State == ConnectionState.Closed))
                {
                    return pooledConnection;
                }
                else
                {
                    // Remove invalid connection from pool
                    ConnectionPool.TryRemove(connectionString, out _);
                }
            }

            // Create new connection if not in pool or invalid
            lock (PoolLock)
            {
                // Double-check after lock
                if (ConnectionPool.TryGetValue(connectionString, out pooledConnection) && 
                    pooledConnection != null && 
                    (pooledConnection.State == ConnectionState.Open || pooledConnection.State == ConnectionState.Closed))
                {
                    return pooledConnection;
                }

                // Check pool size limit before creating new connection
                if (ConnectionPool.Count >= MAX_POOL_SIZE)
                {
                    // Remove oldest connection to make room
                    var oldestKey = ConnectionPool.Keys.FirstOrDefault();
                    if (!string.IsNullOrEmpty(oldestKey) && ConnectionPool.TryRemove(oldestKey, out var oldConnection))
                    {
                        try
                        {
                            if (oldConnection?.State == ConnectionState.Open)
                                oldConnection.Close();
                            oldConnection?.Dispose();
                        }
                        catch { /* Ignore cleanup errors */ }
                    }
                }

                // Create new connection with fallback logic
                var newConnection = CreateConnectionWithFallback();
                if (newConnection != null)
                {
                    ConnectionPool.TryAdd(connectionString, newConnection);
                }
                return newConnection;
            }
        }

        private OleDbConnection CreateConnectionWithFallback()
        {
            string connectionString = this.ConnectionString;
            
            try
            {
                var connection = new OleDbConnection(connectionString);
                // Test the connection
                connection.Open();
                connection.Close();
                return connection;
            }
            catch (Exception ex) when (ex.Message.Contains("provider") || ex.Message.Contains("registered"))
            {
                // If ACE provider fails, try with alternative settings
                FileInfo fi = new FileInfo(this.filepath);
                string fallbackConnectionString;
                
                if (fi.Extension.Equals(".xls"))
                {
                    // Try ACE provider with different Excel version
                    fallbackConnectionString = string.Format(this.excelObject,
                                       "ACE", "16.0", this.filepath, "8.0");
                }
                else
                {
                    // Try ACE provider 16.0 for xlsx
                    fallbackConnectionString = string.Format(this.excelObject,
                                       "ACE", "16.0", this.filepath, "12.0");
                }
                
                try
                {
                    var fallbackConnection = new OleDbConnection(fallbackConnectionString);
                    // Test the fallback connection
                    fallbackConnection.Open();
                    fallbackConnection.Close();
                    return fallbackConnection;
                }
                catch
                {
                    // If all fails, throw original exception with helpful message
                    throw new Exception($"OLEDB Provider Error: Unable to connect to Excel file '{this.filepath}'. " +
                                      $"Original error: {ex.Message}. " +
                                      $"Please ensure Microsoft Access Database Engine is installed. " +
                                      $"Download from: https://www.microsoft.com/en-us/download/details.aspx?id=54920");
                }
            }
        }

        // Reads the Schema Information
        public DataTable GetSchema()
        {
            DataTable dtSchema = null;
            
            // CRITICAL FIX: Validate connection before use
            if (!EnsureConnectionValid())
            {
                string errorDetails = $"Failed to establish a valid database connection to file: {this.filepath}";
                System.Diagnostics.Debug.WriteLine($"GetSchema Error: {errorDetails}");
                throw new InvalidOperationException(errorDetails);
            }
            
            if (this.Connection.State != ConnectionState.Open) 
                this.Connection.Open();
            dtSchema = this.Connection.GetOleDbSchemaTable(
                   OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            return dtSchema;
        }

        //Reads table and returns the DataTable
        public DataTable ReadTable(string tableName)
        {
            return this.ReadTable(tableName, "");
        }

        public DataTable ReadTable(string tableName, string criteria)
        {
            try
            {
                // CRITICAL FIX: Validate and recover connection before use
                if (!EnsureConnectionValid())
                {
                    string errorDetails = $"Failed to establish a valid database connection to file: {this.filepath}. Check if file exists and is accessible.";
                    System.Diagnostics.Debug.WriteLine($"ReadTable Error: {errorDetails}");
                    throw new InvalidOperationException(errorDetails);
                }

                if (this.Connection.State != ConnectionState.Open)
                {
                    this.Connection.Open();
                    onReadProgress(10);
                }

                string cmdText = "Select * from [{0}]";
                if (!string.IsNullOrEmpty(criteria))
                {
                    cmdText += " Where " + criteria;
                }

                OleDbCommand cmd = new OleDbCommand(string.Format(cmdText, tableName));
                cmd.Connection = this.Connection;

                OleDbDataAdapter adpt = new OleDbDataAdapter(cmd);
                onReadProgress(30);

                DataSet ds = new DataSet();
                onReadProgress(50);

                //Fill up dataset 
                adpt.Fill(ds, tableName);
                onReadProgress(100);

                if (ds.Tables.Count == 1)
                {
                    return ds.Tables[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //MessageBox.Show("Table Cannot be read");
                return null;
            }
        }

        /// <summary>
        /// CRITICAL FIX: Direct table read WITHOUT connection validation to prevent connection loops
        /// Use this ONLY when connection has already been validated
        /// </summary>
        public DataTable ReadTableDirect(string tableName, string criteria)
        {
            try
            {
                // ASSUMPTION: Connection is already validated by caller
                if (this.Connection.State != ConnectionState.Open)
                {
                    this.Connection.Open();
                    onReadProgress(10);
                }

                string cmdText = "Select * from [{0}]";
                if (!string.IsNullOrEmpty(criteria))
                {
                    cmdText += " Where " + criteria;
                }

                OleDbCommand cmd = new OleDbCommand(string.Format(cmdText, tableName));
                cmd.Connection = this.Connection;

                OleDbDataAdapter adpt = new OleDbDataAdapter(cmd);
                onReadProgress(30);

                DataSet ds = new DataSet();
                onReadProgress(50);

                //Fill up dataset 
                adpt.Fill(ds, tableName);
                onReadProgress(100);

                if (ds.Tables.Count == 1)
                {
                    return ds.Tables[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReadTableDirect Error: {ex.Message}");
                // Don't show MessageBox to prevent UI spam during batch operations
                return null;
            }
        }

        //Generates DropTable statement and executes it.
        public bool DropTable(string tablename)
        {
            try
            {
                if (this.Connection.State != ConnectionState.Open)
                {
                    this.Connection.Open();
                    onWriteProgress(10);
                }
                string cmdText = "Drop Table [{0}]";
                using (OleDbCommand cmd = new OleDbCommand(
                         string.Format(cmdText, tablename), this.Connection))
                {
                    onWriteProgress(30);

                    cmd.ExecuteNonQuery();
                    onWriteProgress(80);
                }
                this.Connection.Close();
                onWriteProgress(100);

                return true;
            }
            catch (Exception ex)
            {
                onWriteProgress(0);

                MessageBox.Show(ex.Message);
                return false;
            }
        }
        // Creates Create Table Statement and runs it.
        public bool WriteTable(string tableName, Dictionary<string, string>
                                                             tableDefination)
        {
            try
            {
                using (OleDbCommand cmd = new OleDbCommand(
                this.GenerateCreateTable(tableName, tableDefination), this.Connection))
                {
                    if (this.Connection.State != ConnectionState.Open)
                        this.Connection.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        // Generates Insert Statement and executes it
        public bool AddNewRow(DataRow dr)
        {
            using (OleDbCommand cmd = new OleDbCommand(
                          this.GenerateInsertStatement(dr), this.Connection))
            {
                cmd.ExecuteNonQuery();
            }
            return true;
        }
        // Create Table Generation based on Table Defination
        private string GenerateCreateTable(string tableName,
                            Dictionary<string, string> tableDefination)
        {
            StringBuilder sb = new StringBuilder();
            bool firstcol = true;
            sb.AppendFormat("CREATE TABLE [{0}](", tableName);
            firstcol = true;
            foreach (KeyValuePair<string, string> keyvalue in tableDefination)
            {
                if (!firstcol)
                {
                    sb.Append(",");
                }
                firstcol = false;
                sb.AppendFormat("{0} {1}", keyvalue.Key, keyvalue.Value);
            }

            sb.Append(")");
            return sb.ToString();
        }
        //Generates InsertStatement from a DataRow.
        private string GenerateInsertStatement(DataRow dr)
        {
            StringBuilder sb = new StringBuilder();
            bool firstcol = true;
            sb.AppendFormat("INSERT INTO [{0}](", dr.Table.TableName);

            foreach (DataColumn dc in dr.Table.Columns)
            {
                if (!firstcol)
                {
                    sb.Append(",");
                }
                firstcol = false;

                sb.Append(dc.Caption);
            }

            sb.Append(") VALUES(");
            firstcol = true;
            for (int i = 0; i <= dr.Table.Columns.Count - 1; i++)
            {
                if (!object.ReferenceEquals(dr.Table.Columns[i].DataType, typeof(int)))
                {
                    sb.Append("'");
                    sb.Append(dr[i].ToString().Replace("'", "''"));
                    sb.Append("'");
                }
                else
                {
                    sb.Append(dr[i].ToString().Replace("'", "''"));
                }
                if (i != dr.Table.Columns.Count - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append(")");
            return sb.ToString();
        }


        public void Dispose()
        {
            // CONNECTION POOLING: Don't dispose pooled connections, just close if needed
            if (this.con != null && this.con.State == ConnectionState.Open)
            {
                try
                {
                    this.con.Close();
                }
                catch { /* Ignore close errors */ }
            }
            
            // Don't dispose the pooled connection, just clear reference
            this.con = null;
            this.filepath = string.Empty;
        }

        // CONNECTION POOLING: Auto cleanup idle connections
        private static void CleanupIdleConnections(object state)
        {
            lock (PoolLock)
            {
                var now = DateTime.Now;
                var expiredConnections = LastUsedTime
                    .Where(kvp => now - kvp.Value > IDLE_TIMEOUT)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var connectionString in expiredConnections)
                {
                    if (ConnectionPool.TryRemove(connectionString, out var connection))
                    {
                        try
                        {
                            if (connection.State == ConnectionState.Open)
                                connection.Close();
                            connection.Dispose();
                        }
                        catch { /* Ignore cleanup errors */ }
                    }
                    LastUsedTime.TryRemove(connectionString, out _);
                }

                if (expiredConnections.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Cleaned up {expiredConnections.Count} idle connections");
                }
            }
        }

        /// <summary>
        /// FINAL OPTIMIZATION: Add comprehensive connection pool management methods
        /// </summary>
        public static void CleanupConnectionPool()
        {
            try
            {
                CleanupIdleConnections(null);
                System.Diagnostics.Debug.WriteLine("Connection pool cleanup completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection pool cleanup failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// MONITORING: Get detailed pool statistics
        /// </summary>
        public static string GetPoolStatistics()
        {
            var totalConnections = ConnectionPool.Count;
            var activeConnections = ConnectionPool.Count(kvp => kvp.Value.State == ConnectionState.Open);
            var now = DateTime.Now;
            var idleConnections = LastUsedTime.Count(kvp => now - kvp.Value > TimeSpan.FromMinutes(5));
            
            return $"Total: {totalConnections}, Active: {activeConnections}, Idle: {idleConnections}";
        }
        
        /// <summary>
        /// UTILITY: Get connection pool size for monitoring
        /// </summary>
        public static int GetPoolSize()
        {
            return ConnectionPool?.Count ?? 0;
        }

        /// <summary>
        /// CRITICAL FIX: Ensure connection is valid and recreate if necessary
        /// Prevents crash when connection is corrupted from previous operations
        /// </summary>
        private bool EnsureConnectionValid()
        {
            try
            {
                // Check if connection exists and is in a valid state
                if (this.con == null)
                {
                    System.Diagnostics.Debug.WriteLine("Connection is null, recreating...");
                    return RecreateConnection();
                }

                // Check if connection is disposed or broken
                try
                {
                    // Try to access connection state to test if it's valid
                    var state = this.con.State;
                    
                    // If connection is broken, recreate it
                    if (state == ConnectionState.Broken)
                    {
                        System.Diagnostics.Debug.WriteLine("Connection is broken, recreating...");
                        return RecreateConnection();
                    }
                    
                    return true;
                }
                catch (ObjectDisposedException)
                {
                    System.Diagnostics.Debug.WriteLine("Connection is disposed, recreating...");
                    return RecreateConnection();
                }
                catch (InvalidOperationException)
                {
                    System.Diagnostics.Debug.WriteLine("Connection is invalid, recreating...");
                    return RecreateConnection();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validating connection: {ex.Message}");
                return RecreateConnection();
            }
        }

        /// <summary>
        /// CRITICAL FIX: Recreate connection from scratch with enhanced error reporting
        /// </summary>
        private bool RecreateConnection()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== RECREATING CONNECTION ===");
                System.Diagnostics.Debug.WriteLine($"File path: {this.filepath}");
                System.Diagnostics.Debug.WriteLine($"File exists: {(!string.IsNullOrEmpty(this.filepath) ? File.Exists(this.filepath).ToString() : "N/A")}");

                // Dispose old connection safely
                if (this.con != null)
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Disposing old connection (state: {this.con.State})");
                        if (this.con.State == ConnectionState.Open)
                            this.con.Close();
                        this.con.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing old connection: {ex.Message}");
                    }
                }

                // Validate file access
                if (string.IsNullOrEmpty(this.filepath))
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: File path is null or empty");
                    return false;
                }

                if (!File.Exists(this.filepath))
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: File does not exist: {this.filepath}");
                    return false;
                }

                // Check file access permissions
                try
                {
                    using (var fs = File.Open(this.filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // File is accessible
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Cannot access file: {ex.Message}");
                    return false;
                }

                // Create new connection
                string connectionString = this.ConnectionString;
                System.Diagnostics.Debug.WriteLine($"Connection string: {connectionString}");
                
                this.con = new OleDbConnection(connectionString);
                
                // Test the connection
                try
                {
                    this.con.Open();
                    System.Diagnostics.Debug.WriteLine("Connection opened successfully");
                    this.con.Close();
                    System.Diagnostics.Debug.WriteLine("Connection recreated and tested successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Cannot open connection: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Exception in RecreateConnection: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// CRITICAL FIX: Force cleanup of entire connection pool to prevent corruption
        /// Call this before batch search operations
        /// IMPROVED: Preserve individual instance connections
        /// </summary>
        public static void ForceCleanupAllConnections()
        {
            lock (PoolLock)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Force cleanup: Disposing {ConnectionPool.Count} pooled connections");
                    
                    // Dispose pooled connections only (not individual instance connections)
                    var connectionsToDispose = new List<OleDbConnection>();
                    foreach (var connection in ConnectionPool.Values)
                    {
                        if (connection != null)
                            connectionsToDispose.Add(connection);
                    }
                    
                    // Clear pools first
                    ConnectionPool.Clear();
                    LastUsedTime.Clear();
                    
                    // Then dispose connections
                    foreach (var connection in connectionsToDispose)
                    {
                        try
                        {
                            if (connection.State == ConnectionState.Open)
                                connection.Close();
                            connection.Dispose();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error disposing pooled connection: {ex.Message}");
                        }
                    }
                    
                    // Force garbage collection
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    
                    System.Diagnostics.Debug.WriteLine("Force cleanup completed - individual instances preserved");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in force cleanup: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// DEBUG: Get pool status for troubleshooting (already exists above)
        /// </summary>
        // Removed duplicate GetPoolSize method

        /// <summary>
        /// CRITICAL FIX: Public wrapper for connection validation to prevent external connection loops
        /// Use this before batch operations to validate connection once
        /// </summary>
        public bool ValidateConnection()
        {
            return EnsureConnectionValid();
        }
    }
}
