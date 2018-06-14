using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.Threading;
using Core;
using System.Net;
using System.Reflection;
using System.IO;
using ProtoBuf;
using System.Net.Sockets;

namespace Core
{ 
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class MsgHTTP : IMsgSubcribe
    {
        private int PortHost = 10101;
        private int PortClient = 0;

        private readonly IMsgStore store;
        private readonly HttpListener listener;
        public void Close()
        {
            listener.Close();
        }

        public MsgHTTP(IMsgStore store_, bool isHost = false)
        {
            if (isHost) PortClient = PortHost;
            store = store_;

            ///////////////////////////////////////////////////////////////////////
            // HTTP LISTENER
            listener = new HttpListener();
            TcpListener lis = new TcpListener(IPAddress.Loopback, PortClient);
            lis.Start();
            PortClient = ((IPEndPoint)lis.LocalEndpoint).Port;
            lis.Stop();
            listener.Prefixes.Add("http://*:" + PortClient.ToString() + "/");
            listener.Start();
            new Thread(() =>
            {
                while (true)
                {
                    HttpListenerContext ctx = listener.GetContext();
                    ThreadPool.QueueUserWorkItem((_) => ProcessRequest(ctx));
                }
            }).Start(); 
        }

        void ProcessRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.LocalPath;
            if (path == "/favicon.ico")
            {
                context.Response.Close();
                return;
            }
            try
            {
                Msg m = null;
                if (context.Request.HttpMethod == "POST")
                {
                    Type requestType = typeof(Msg);
                    m = (Msg)Serializer.NonGeneric.Deserialize(requestType, context.Request.InputStream);
                    if (m != null)
                    {
                        Serializer.NonGeneric.Serialize(context.Response.OutputStream, 200);
                        context.Response.Close();
                        store.Add(m);
                    }
                }
            }
            catch
            {
                Serializer.NonGeneric.Serialize(context.Response.OutputStream, 500);
                context.Response.Close();
            }
        }

        public void Send(object data)
        {
            using (var client = new WebClient() { BaseAddress = "http://127.0.0.1:" + PortHost.ToString() })
            {
                byte[] buf;
                using (var ms = new MemoryStream())
                {
                    Serializer.NonGeneric.Serialize(ms, data);
                    buf = ms.ToArray();
                }
                client.UploadData("/", buf);
            }
        }

        public T Send<T>(object data)
        {
            T val;
            using (var client = new WebClient() { BaseAddress = "http://127.0.0.1:" + PortHost.ToString() })
            {
                byte[] buf;
                using (var ms = new MemoryStream())
                {
                    Serializer.NonGeneric.Serialize(ms, data);
                    buf = ms.ToArray();
                }
                buf = client.UploadData("/", buf); // data is now the response
                using (var ms = new MemoryStream(buf))
                    val = Serializer.Deserialize<T>(ms);
            }
            return val;
        }

        public T Get<T>(string key)
        {
            T val;
            using (var client = new WebClient() { BaseAddress = "http://127.0.0.1:" + PortHost.ToString() })
            {
                byte[] buf = client.DownloadData("/" + key);
                using (var ms = new MemoryStream(buf))
                    val = Serializer.Deserialize<T>(ms);
            }
            return val;
        }

        public void SendText(string text)
        {
             
        }
    }//end class 
}
