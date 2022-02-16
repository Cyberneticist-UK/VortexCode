
using System;
using System.Data;

namespace Vortex
{
    /// <summary>
    /// Designed to connect to a MySQL Database!
    /// </summary>
    public class Comms_DatabaseMySQL : Interface_DataConnector
    {
       protected MySql.Data.MySqlClient.MySqlDataAdapter DataAdapter;

        public string ErrorMessage { get; set; } = "";
        public bool IsBusy { get; set; } = false;
        
        public Comms_DatabaseMySQL(string Server, string Catalogue, string Username = "", string Password = "")
        {

            string ConnectionString = @"server="+Server+";userid="+Username+";password="+Password+";database="+Catalogue;
            DataAdapter = new MySql.Data.MySqlClient.MySqlDataAdapter();
            MySql.Data.MySqlClient.MySqlConnection Connection1 = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
            this.DataAdapter.SelectCommand = new MySql.Data.MySqlClient.MySqlCommand("", Connection1);
            this.DataAdapter.InsertCommand = new MySql.Data.MySqlClient.MySqlCommand("", Connection1);
        }

        public void DataOpen()
        {
            try
            {
                if (DataAdapter.SelectCommand.Connection.State == ConnectionState.Closed)
                    DataAdapter.SelectCommand.Connection.Open();
                if (DataAdapter.InsertCommand.Connection.State == ConnectionState.Closed)
                    DataAdapter.InsertCommand.Connection.Open();
                IsBusy = true;
            }
            catch
            {
            }
        }

        public void DataClose()
        {
            try
            {
                if (DataAdapter.SelectCommand.Connection.State == ConnectionState.Open)
                    DataAdapter.SelectCommand.Connection.Close();
                if (DataAdapter.InsertCommand.Connection.State == ConnectionState.Open)
                    DataAdapter.InsertCommand.Connection.Close();
                IsBusy = false;
            }
            catch
            {
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
            int Result = 0;
            try
            {
                Result = DataAdapter.InsertCommand.ExecuteNonQuery();
                ErrorMessage = "";
            }
            catch (Exception err)
            {
                ErrorMessage = System.DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + " " + err.Message;
            }
            return Result;
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
                DataAdapter.SelectCommand.CommandText = Query;
                object test = DataAdapter.SelectCommand.ExecuteScalar();
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
            if (dt == null)
                dt = new DataTable();
            try
            {
                DataAdapter.SelectCommand.CommandText = Query;
                DataAdapter.Fill(dt);
            }
            catch (Exception err)
            {
                ErrorMessage = "Error in DataConnector DataScalar Query: <b>" + Query + "</b>: " + err.Message + "<br />";
            }
            return dt;
        }
    }
}
