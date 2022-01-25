using System;
using System.Data;
using System.Data.SqlClient;

namespace Vortex
{
    public class Comms_DatabaseSQLServer: Interface_DataConnector
    {
        protected SqlDataAdapter DataAdapter;

        public string ErrorMessage { get; set; } = "";

        public bool IsBusy { get; set; } = false;


        public Comms_DatabaseSQLServer(string Server, string Catalogue, string Username = "", string Password = "")
        {

            string ConnectionString = @"data source=" + Server + "; initial catalog = " + Catalogue + "; uid=" + Username + "; pwd=" + Password + ";";
            if (Username == "" && Password == "")
                ConnectionString = @"data source=" + Server + "; initial catalog = " + Catalogue + "; Integrated Security=True;";
            DataAdapter = new SqlDataAdapter();
            SqlConnection Connection1 = new SqlConnection(ConnectionString);
            this.DataAdapter.SelectCommand = new SqlCommand("", Connection1);
            this.DataAdapter.InsertCommand = new SqlCommand("", Connection1);
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
            DataAdapter.InsertCommand.CommandText = Query;
            int Result = DataNonSelectBatch(Query);
            DataClose();
            return Result;
        }

        public int DataNonSelectBatch(string Query)
        {
            int Result = 0;
            try
            {
                DataAdapter.InsertCommand.CommandText = Query;
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
            dt.TableName = "Result";
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
            dt.TableName = "Result";
            return dt;
        }
    }
}
