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
    public class MsgListener : IMsgSubcribe
    {
        private int PortHost = 10101;
        private int PortClient = 0;

        private readonly IMsgStore store;
        private readonly HttpListener listener;
        public void Close()
        {
            listener.Close();
        }

        public MsgListener(IMsgStore store_, bool isHost = false)
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
            listener.BeginGetContext(ProcessRequest, listener);
        }

        private void ProcessRequest(IAsyncResult result)
        {
            // http://stackoverflow.com/questions/427326/httplistener-server-header-c-sharp

            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);

            string method = context.Request.HttpMethod;
            string path = context.Request.Url.LocalPath;
            switch (path)
            {
                case "/favicon.ico":
                    context.Response.Close();
                    return;
                case "/ping":
                    byte[] buffer = Encoding.UTF8.GetBytes("OK");
                    context.Response.ContentLength64 = buffer.Length; 
                    System.IO.Stream output = context.Response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                    break;
                default:
                    if (method == "POST")
                    {
                        try
                        {
                            Msg m = null;
                            Type requestType = typeof(Msg);
                            m = (Msg)Serializer.NonGeneric.Deserialize(requestType, context.Request.InputStream);
                            if (m != null)
                            {
                                Serializer.NonGeneric.Serialize(context.Response.OutputStream, 200);
                                context.Response.Close();
                                store.Add(m);
                            }
                        }
                        catch
                        {
                            Serializer.NonGeneric.Serialize(context.Response.OutputStream, 500);
                            context.Response.Close();
                        }
                    }
                    break;
            }

            listener.BeginGetContext(ProcessRequest, listener);
        }

        //////private void ProcessRequest(IAsyncResult result)
        //////{
        //////    // http://stackoverflow.com/questions/427326/httplistener-server-header-c-sharp

        //////    HttpListener listener = (HttpListener)result.AsyncState;
        //////    HttpListenerContext context = listener.EndGetContext(result);

        //////    string responseString = "<html>Hello World</html>";
        //////    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        //////    context.Response.ContentLength64 = buffer.Length;

        //////    //One
        //////    context.Response.AddHeader("Server", "My Personal Server");

        //////    //Two
        //////    context.Response.Headers.Remove(HttpResponseHeader.Server);
        //////    context.Response.Headers.Add(HttpResponseHeader.Server, "My Personal Server");

        //////    //Three
        //////    context.Response.Headers.Set(HttpResponseHeader.Server, "My Personal Server");

        //////    System.IO.Stream output = context.Response.OutputStream;
        //////    output.Write(buffer, 0, buffer.Length);
        //////    output.Close();

        //////    listener.BeginGetContext(ProcessRequest, listener);
        //////}
         

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
            throw new NotImplementedException();
        }
    }//end class 
}
