using Fleck2.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using WebSocketProxy;
using Fleck2;
using System.Net.Sockets;
using System.Threading;

namespace App
{
    public class MsgHost
    {
        public static int Port = 8080; //FreeTcpPort();

        static int portHTTP = FreeTcpPort(), portWebSocket = FreeTcpPort();

        public static void Init()
        {
            new Thread(new ThreadStart(() =>
            {
                TcpProxyConfiguration config = new TcpProxyConfiguration()
                {
                    PublicHost = new Host(IPAddress.Parse("0.0.0.0"), Port),
                    HttpHost = new Host(IPAddress.Loopback, portHTTP),
                    WebSocketHost = new Host(IPAddress.Loopback, portWebSocket),
                };
                
                var httpHost = new HttpProxyServer();
                var wsServer = new WebSocketServer(string.Format("ws://0.0.0.0:{0}", portWebSocket));
                var tcpProxy = new TcpProxyServer(config);

                wsServer.Start(client =>
                {
                    client.OnOpen = () => { try { MsgSocket.OnOpen(client); } catch { } };
                    client.OnClose = () => { try { MsgSocket.OnClose(client); } catch { } };
                    client.OnMessage = msg => { try { MsgSocket.OnMessage(client, msg); } catch { } };
                    client.OnBinary = rawData => { try { MsgSocket.OnBinary(client, rawData); } catch { } };
                    client.OnError = ex => { try { MsgSocket.OnError(client, ex); } catch { } };
                });

                httpHost.Start(portHTTP);
                //httpHost.Start(string.Format("http://localhost:{0}/", portHTTP));

                tcpProxy.Start();

                while (true) { }
            })).Start();
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
