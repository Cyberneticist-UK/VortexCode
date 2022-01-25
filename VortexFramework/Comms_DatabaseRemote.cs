using System.Data;

namespace Vortex
{
    public class Comms_DatabaseRemote : Interface_DataConnector
    {
        webClient.DirectSoapClient Client = new webClient.DirectSoapClient();

        public string ErrorMessage { get; set; } = "";
        public bool IsBusy { get; set; } = false;

        private string EncryptQuery(string Query)
        {
            return Sec_Crypt.AESEncrypt(Query, "WebBasedEncryptionKeyGiraffeMongoose");
        }

        private string Decryptstring(string Query)
        {
            return Sec_Crypt.AESDecrypt(Query, "WebBasedEncryptionKeyGiraffeMongoose");
        }

        public void DataOpen()
        {

        }

        public void DataClose()
        {
            
        }

        public int DataNonSelect(string Query)
        {
            return DataNonSelectBatch(Query);
        }

        public int DataNonSelectBatch(string Query)
        {
            return Client.QueryNonSelect(EncryptQuery(Query));
        }
        
        public string DataScalar(string Query)
        {
            return DataScalarBatch(Query);
        }

        public string DataScalarBatch(string Query)
        {
            string X = Client.QueryScalar(EncryptQuery(Query));
            return Decryptstring(X);
        }

        public DataTable DataSelect(string Query, DataTable dt = null)
        {
            return DataSelectBatch(Query, dt);
        }

        public DataTable DataSelectBatch(string Query, DataTable dt = null)
        {
            return Client.QuerySelect(EncryptQuery(Query));
        }
    }
}
