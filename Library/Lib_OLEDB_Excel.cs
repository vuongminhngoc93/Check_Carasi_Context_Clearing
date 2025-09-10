using System.IO;
using System.Data.OleDb;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;

namespace Check_carasi_DF_ContextClearing
{
    class Lib_OLEDB_Excel : IDisposable
    {
        /*Author: Vuong Minh Ngoc (MS/EJV)
        Version: 1.0.0
        Description: transfer data from Excel to OLEDB*/

        // CONNECTION POOLING: Static pool to reuse connections
        private static readonly ConcurrentDictionary<string, OleDbConnection> ConnectionPool = 
            new ConcurrentDictionary<string, OleDbConnection>();
        private static readonly object PoolLock = new object();

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

        // CONNECTION POOLING: Static method to cleanup all pooled connections
        public static void CleanupConnectionPool()
        {
            lock (PoolLock)
            {
                foreach (var kvp in ConnectionPool)
                {
                    try
                    {
                        if (kvp.Value != null)
                        {
                            if (kvp.Value.State == ConnectionState.Open)
                                kvp.Value.Close();
                            kvp.Value.Dispose();
                        }
                    }
                    catch { /* Ignore cleanup errors */ }
                }
                ConnectionPool.Clear();
            }
        }

        // CONNECTION POOLING: Get pool statistics for monitoring
        public static int GetPoolSize()
        {
            return ConnectionPool.Count;
        }
    }
}
