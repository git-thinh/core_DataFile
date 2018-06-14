using System.Net;

namespace WebSocketProxy
{
    public class Host
    {
        public Host(IPAddress ip, int port)
        {
            IpAddress = ip;
            Port = port;
        }

        public IPAddress IpAddress { get; set; }

        public int Port { get; set; }

        public bool IsSpecified
        {
            get
            {
                return Port != -1 && IpAddress != null;
            } 
        } 

        public override string ToString()
        {
            return IpAddress + ":" + Port;
        }
    }
}