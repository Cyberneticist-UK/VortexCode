using System.Data;

namespace Vortex
{
    /// <summary>
    /// A Generic interface for data connection objects to databases
    /// </summary>
    public interface Interface_DataConnector
    {
        string ErrorMessage { get; set; }
        bool IsBusy { get; set; }

        void DataOpen();
        void DataClose();

        int DataNonSelect(string Query);
        int DataNonSelectBatch(string Query);

        string DataScalar(string Query);
        string DataScalarBatch(string Query);

        DataTable DataSelect(string Query, DataTable dt = null);
        DataTable DataSelectBatch(string Query, DataTable dt = null);
        
    }
}
