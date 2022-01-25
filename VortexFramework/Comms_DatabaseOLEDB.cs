using System;
using System.Data;
using System.Data.OleDb;

namespace Vortex
{
    /// <summary>
    /// A Standard OLEDB Connector!
    /// </summary>
    public class Comms_DatabaseOLEDB : Interface_DataConnector
    {
        protected OleDbDataAdapter DataAdapter1 = new OleDbDataAdapter();

        public string ErrorMessage { get; set; } = "";
        public bool IsBusy { get; set; } = false;


        public Comms_DatabaseOLEDB(string DatabaseName, string Password = "")
        {
            string ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source='" + DatabaseName + "';Persist Security Info=False;";
            if(Password != "")
                ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;User ID=Admin;Data Source='" + DatabaseName + "';Jet OLEDB:Database Password=" + Password + ";";
            OleDbConnection Connection1 = new OleDbConnection(ConnectionString);
            this.DataAdapter1.SelectCommand = new OleDbCommand("", Connection1);
            this.DataAdapter1.InsertCommand = new OleDbCommand("", Connection1);
        }

        public void DataOpen()
        {
            try
            {
                if (DataAdapter1.SelectCommand.Connection.State == ConnectionState.Closed)
                    DataAdapter1.SelectCommand.Connection.Open();
                if (DataAdapter1.InsertCommand.Connection.State == ConnectionState.Closed)
                    DataAdapter1.InsertCommand.Connection.Open();
                IsBusy = true;
            }
            catch(Exception err)
            {
                ErrorMessage = err.ToString();
            }
        }

        public void DataClose()
        {
            try
            {
                if(DataAdapter1.SelectCommand.Connection.State == ConnectionState.Open)
                    DataAdapter1.SelectCommand.Connection.Close();
                if (DataAdapter1.InsertCommand.Connection.State == ConnectionState.Open)
                    DataAdapter1.InsertCommand.Connection.Close();
                IsBusy = false;
            }
            catch (Exception err)
            {
                ErrorMessage = err.ToString();
            }
        }

        public int DataNonSelect(string Query)
        {
            DataOpen();
            int Result = DataNonSelectBatch(Query);
            DataClose();
            return Result;
        }

        public int DataNonSelectBatch(string Query)
        {
            try
            {
                DataAdapter1.InsertCommand.CommandText = Query;
                return DataAdapter1.InsertCommand.ExecuteNonQuery();
            }
            catch
            {
                return 0;
            }
        }
        
        public string DataScalar(string Query)
        {
            DataOpen();
            string item = DataScalarBatch(Query);
            DataClose();
            return item;
        }

        public string DataScalarBatch(string Query)
        {
            string Scalar = "";
            try
            {
                DataAdapter1.SelectCommand.CommandText = Query;
                object test = DataAdapter1.SelectCommand.ExecuteScalar();
                if (test != null)
                    Scalar = test.ToString();
            }
            catch (Exception err)
            {
                Scalar = "";
                ErrorMessage = "Error in DataConnector DataScalar Query: <b>" + Query + "</b>: " + err.Message + "<br />";
            }
            return Scalar;
        }
        
        public DataTable DataSelect(string Query, DataTable dt = null)
        {
            DataOpen();
            dt = DataSelectBatch(Query, dt);
            DataClose();
            return dt;
        }

        public DataTable DataSelectBatch(string Query, DataTable dt = null)
        {
            if(dt == null)
                dt = new DataTable();
            try
            {
                DataAdapter1.SelectCommand.CommandText = Query;
                DataAdapter1.Fill(dt);
            }
            catch (Exception err)
            {
                ErrorMessage = "Error in DataConnector DataScalar Query: <b>" + Query + "</b>: " + err.Message + "<br />";
            }
            return dt;
        }
    }
}
