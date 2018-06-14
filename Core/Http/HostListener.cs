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
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"),
    PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class HostListener
    {
        public int PortHost = 10101;
        private readonly Type msgType = typeof(Msg);

        private readonly ILog log;
        private readonly HttpListener listener;
        private readonly MsgBuffer buffer;
        private readonly DbStore db;

        public HostListener(ILog _log)
        {
            log = _log;
            db = new DbStore(_log);
            buffer = new MsgBuffer(_log, db);

            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + PortHost.ToString() + "/");
        }

        public void Start()
        {
            listener.Start();
            listener.BeginGetContext(ProcessRequest, listener);
        }

        private void ProcessRequest(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);

            string method = context.Request.HttpMethod;
            string path = context.Request.Url.LocalPath;
            switch (method)
            {
                case "PUT":
                    #region [ === PUT === ]

                    try
                    {
                        Msg m = null;
                        m = (Msg)Serializer.NonGeneric.Deserialize(msgType, context.Request.InputStream);
                        if (m != null)
                        {
                            Serializer.NonGeneric.Serialize(context.Response.OutputStream, 200);
                            context.Response.Close();
                            buffer.Receive(m);
                        }
                    }
                    catch
                    {
                        Serializer.NonGeneric.Serialize(context.Response.OutputStream, 500);
                        context.Response.Close();
                    }

                    break;
                    #endregion
                case "POST":
                    #region [ === PUT === ]

                    try
                    {
                        Msg m = null;
                        //m = (Msg)Serializer.NonGeneric.Deserialize(msgType, context.Request.InputStream);
                        m = ProtoBuf.Serializer.Deserialize<Msg>(context.Request.InputStream);
                        if (m != null)
                        {
                            Serializer.NonGeneric.Serialize(context.Response.OutputStream, 200);
                            //context.Response.Close();
                            context.Response.Abort(); 
                            buffer.ReceiveNewThread(m);
                        }
                    }
                    catch(Exception ex)
                    {
                        //Serializer.NonGeneric.Serialize(context.Response.OutputStream, 500);
                        //context.Response.Close();
                    }

                    break;
                    #endregion
                case "GET":
                    #region [ === GET === ]

                    switch (path)
                    {
                        case "/favicon.ico":
                            context.Response.Close();
                            return;
                        case "/ping":
                            byte[] buffer = Encoding.UTF8.GetBytes("OK");
                            context.Response.ContentLength64 = buffer.Length;
                            Stream output = context.Response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            output.Close();
                            break;
                    }

                    break;
                    #endregion
            }

            listener.BeginGetContext(ProcessRequest, listener);
        }

        public void Close()
        {
            listener.Close();
        }

    }//end class 
}
