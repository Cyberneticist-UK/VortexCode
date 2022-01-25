using System;

namespace Vortex
{
    public delegate void MessageTransfer(byte[] Message);
    public delegate byte[] WebServerSentMessage(Guid Port, string URL, string Data);
    public delegate void StringTransfer(string Message);
    public delegate void BlankTransfer();
    public delegate void SendData(TransportMessage Data);
}
