
namespace Vortex
{
    /// <summary>
    /// I can't currently remember what a blank interface is for. Sorry!
    /// Version 1.0
    /// 27/02/2018
    /// </summary>
    class Comms_Blank : Comms_Interface_Connection
    {

        public Comms_Blank(TransportPort Port) : base(Port)
        {
            
        }

        public override void CloseConnection()
        {

        }

    }
}
