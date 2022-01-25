using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Vortex
{
    class Comms_WebServer : Comms_Interface_Connection
    {
        public Comms_WebServer(TransportPort Port) : base(Port)
        {
            Protocol = ConnectionProtocol.WebServer;
            StartListen();
        }

        public override void CloseConnection()
        {
            EndListen();
        }

        public event WebServerSentMessage WebMessageReceived;

        HttpListener Listener = new HttpListener();
        /// <summary>
        /// Hide the shutdown token so cannot be seen outside the server
        /// </summary>
        private string ShutDownToken = System.Guid.NewGuid().ToString();

        private Thread httpListenerThread;
        bool Listen = true;

        public void StartListen()
        {
            Listener.Prefixes.Add("http://+:80/Temporary_listen_Addresses/" + Port.ToGuid().ToString() + "/");
            Listener.Start();
            httpListenerThread = new Thread(new ParameterizedThreadStart(StartHttpListenerThread));
            httpListenerThread.Start();
        }

        public void EndListen()
        {
            Listen = false;
            try
            {
                // send it a message you wish to shutdown!
                WebClient c = new WebClient();
                c.DownloadData("http://" + Comms_General.GetMyIP() + "/Temporary_listen_Addresses/" + Port.ToGuid().ToString() + "/Shutdown" + ShutDownToken);
            }
            finally
            {
                Listener.Stop();
                Listener.Prefixes.Remove("http://+:80/Temporary_listen_Addresses/" + Port.ToGuid().ToString() + "/");
            }
        }

        private void StartHttpListenerThread(object s)
        {
            while (Listener.IsListening && Listen == true)
            {
                // blocks until a client has connected to the server
                ProcessRequest();
            }
        }

        private void ProcessRequest()
        {
            var result = Listener.BeginGetContext(ListenerCallback, Listener);
            result.AsyncWaitHandle.WaitOne();
        }

        private void ListenerCallback(IAsyncResult result)
        {
            var context = Listener.EndGetContext(result);
            using (System.IO.Stream output = context.Response.OutputStream)
            {
                try
                {
                    string URL = context.Request.Url.ToString().Substring(context.Request.Url.ToString().LastIndexOf(Port.ToGuid().ToString() + "/") + Port.ToGuid().ToString().Length + 1);
                    URL = URL.Replace("Default.aspx", "");
                    if (URL == "Shutdown" + ShutDownToken)
                    {
                        Listen = false;
                        context.Response.StatusCode = 200;
                        context.Response.StatusDescription = "OK";
                        context.Response.Close();
                    }
                    else
                    {
                        using (StreamReader tmp = new StreamReader(context.Request.InputStream,
                        context.Request.ContentEncoding))
                        {
                            string data_text = tmp.ReadToEnd();

                            context.Response.StatusCode = 200;
                            context.Response.StatusDescription = "OK";
                            //use this line to get your custom header data in the request.
                            //var headerText = context.Request.Headers["mycustomHeader"];
                            byte[] StreamData = WebMessageReceived(Port.ToGuid(), URL, data_text);
                            output.Write(StreamData, 0, StreamData.Length);
                            //use this line to send your response in a custom header
                            //context.Response.Headers["mycustomResponseHeader"] = "mycustomResponse";
                            context.Response.Close();
                        }
                    }
                }
                catch (Exception err)
                {
                    string X = "<h1>Ooops!</h1><p>" + err.Message + "</p>";
                    context.Response.StatusCode = 200;
                    context.Response.StatusDescription = "OK";
                    output.Write(Encoding.ASCII.GetBytes(X), 0, X.Length);
                    context.Response.Close();
                }
            }
        }

        


    }
}
