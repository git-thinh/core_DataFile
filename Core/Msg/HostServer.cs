using Fleck2.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using WebSocketProxy;
using Fleck2;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using ProtoBuf;

namespace Core
{ 
    public class HostServer 
    {
        public int Port = 10101;
        public int PortHTTP = FreeTcpPort(), PortWebSocket = FreeTcpPort();

        public delegate void OnClientEvent(MsgConnectEvent TypeEvent, long client_id, long msg_id);
        public event OnClientEvent OnClient;

        private readonly ILog log;
        private readonly HostMsg host;
        public HostServer(ILog _log)
        {
            log = _log;
            host = new HostMsg(_log);
        }
         

        public void Start()
        {
            TcpProxyConfiguration config = new TcpProxyConfiguration()
            {
                PublicHost = new Host(IPAddress.Parse("0.0.0.0"), Port),
                HttpHost = new Host(IPAddress.Loopback, PortHTTP),
                WebSocketHost = new Host(IPAddress.Loopback, PortWebSocket),
            };

            var httpHost = new HttpProxyServer();
            var wsServer = new WebSocketServer(string.Format("ws://0.0.0.0:{0}", PortWebSocket));
            var tcpProxy = new TcpProxyServer(config);

            wsServer.Start(connect =>
            {
                connect.OnOpen = () => { try { host.OnOpen(connect); } catch { } };
                connect.OnClose = () => { try { host.OnClose(connect.ConnectionInfo.Id); } catch { } };
                //connect.OnMessage = msg => { try { OnMessage(connect, msg); } catch { } };
                connect.OnBinary = rawData => { try { host.OnReceiveBinary(rawData); } catch { } };
                //connect.OnError = ex => { try { OnError(connect, ex); } catch { } };
            });

            httpHost.Start(PortHTTP);
            //httpHost.Start(string.Format("http://localhost:{0}/", portHTTP));

            tcpProxy.Start();
        }

        private static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

    }
}
