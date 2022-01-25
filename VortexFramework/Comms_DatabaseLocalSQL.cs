using System;
using System.Data;
using System.Data.SqlClient;

namespace Vortex
{
    /// <summary>
    /// Designed to connect to an MDB file!
    /// </summary>
    public class Comms_DatabaseLocalSQL : Interface_DataConnector
    {
        protected SqlDataAdapter DataAdapter;

        public string ErrorMessage { get; set; } = "";
        public bool IsBusy { get; set; } = false;
        
        public Comms_DatabaseLocalSQL(string Filename)
        {
            string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename=" + Filename + "; Integrated Security=True; Connect Timeout=30";
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
