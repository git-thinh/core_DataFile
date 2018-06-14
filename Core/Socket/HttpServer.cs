using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Linq;

namespace Core
{
    public delegate void delReceiveWebRequest(HttpListenerContext Context);

    /// <summary>
    /// Wrapper class for the HTTPListener to allow easier access to the
    /// server, for start and stop management and event routing of the actual
    /// inbound requests.
    /// </summary>
    public class HttpServer : IDisposable
    {
        protected HttpListener Listener;
        protected bool IsStarted = false;

        public event delReceiveWebRequest ReceiveWebRequest;

        public HttpServer()
        {
        }

        public void Start(int port)
        {
            // *** Already running - just leave it in place
            if (this.IsStarted)
                return;

            if (this.Listener == null)
            {
                this.Listener = new HttpListener();
            }

            string[] us = new string[] {
                string.Format("http://127.0.0.1:{0}/", port),
                string.Format("http://localhost:{0}/", port),
            };
            foreach (string url in us)
                this.Listener.Prefixes.Add(url);

            String strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            var addr = ipEntry.AddressList.Select(x => string.Format("http://{0}:{1}/", x.ToString(), port))
                .Where(x => x.Split('.').Length == 4 && !x.Contains(".0."))
                .ToArray();

            foreach (string url in addr)
                this.Listener.Prefixes.Add(url);

            this.IsStarted = true;
            this.Listener.Start();

            IAsyncResult result = this.Listener.BeginGetContext(new AsyncCallback(WebRequestCallback), this.Listener);

        }

        /// <summary>
        /// Starts the Web Service
        /// </summary>
        /// <param name="UrlBase">
        /// A Uri that acts as the base that the server is listening on.
        /// Format should be: http://127.0.0.1:8080/ or http://127.0.0.1:8080/somevirtual/
        /// Note: the trailing backslash is required! For more info see the
        /// HttpListener.Prefixes property on MSDN.
        /// </param>
        public void Start(string UrlBase)
        {
            // *** Already running - just leave it in place
            if (this.IsStarted)
                return;

            if (this.Listener == null)
            {
                this.Listener = new HttpListener();
            }

            this.Listener.Prefixes.Add(UrlBase);

            this.IsStarted = true;
            this.Listener.Start();

            IAsyncResult result = this.Listener.BeginGetContext(new AsyncCallback(WebRequestCallback), this.Listener);
        }

        /// <summary>
        /// Shut down the Web Service
        /// </summary>
        public void Stop()
        {
            if (Listener != null)
            {
                this.Listener.Close();
                this.Listener = null;
                this.IsStarted = false;
            }
        }


        protected void WebRequestCallback(IAsyncResult result)
        {
            if (this.Listener == null)
                return;

            // Get out the context object
            HttpListenerContext context = this.Listener.EndGetContext(result);

            // *** Immediately set up the next context
            this.Listener.BeginGetContext(new AsyncCallback(WebRequestCallback), this.Listener);

            if (this.ReceiveWebRequest != null)
                this.ReceiveWebRequest(context);

            this.ProcessRequest(context);
        }

        /// <summary>
        /// Overridable method that can be used to implement a custom hnandler
        /// </summary>
        /// <param name="Context"></param>
        protected virtual void ProcessRequest(HttpListenerContext Context)
        {
        }

        public void Dispose()
        {
            if (this.Listener.IsListening)
            {
                Listener.Stop();
            }
        }
    }

    public class HttpProxyServer : HttpServer
    {
        protected override void ProcessRequest(System.Net.HttpListenerContext Context)
        {
            HttpListenerRequest Request = Context.Request;
            HttpListenerResponse Response = Context.Response;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Request.HttpMethod + " " + Request.RawUrl + " Http/" + Request.ProtocolVersion.ToString());

            if (Request.UrlReferrer != null)
                sb.AppendLine("Referer: " + Request.UrlReferrer);

            if (Request.UserAgent != null)
                sb.AppendLine("User-Agent: " + Request.UserAgent);

            for (int x = 0; x < Request.Headers.Count; x++)
            {
                sb.AppendLine(Request.Headers.Keys[x] + ":" + " " + Request.Headers[x]);
            }

            sb.AppendLine();

            string htm = "<html><body><h1>Hello world</h1>Time is: " + DateTime.Now.ToString() + "<pre>" + sb.ToString() + "</pre>";

            string path = Request.Url.LocalPath.ToLower();
            if (path == "" || path == "/") path = "/index.html";
            if (path == "/ok")
            {
                htm = "OK";
                Response.ContentType = "text/plain";
            }
            else
            {
                //htm = ResourceHelper.GetEmbeddedResource("Test" + path);
                //if (!string.IsNullOrEmpty(htm))
                //{
                //    htm = htm.Replace("{###}", MsgHost.Port.ToString());
                //}
                //else htm = "";
                Response.ContentType = "text/html";
            }

            byte[] buf = System.Text.Encoding.UTF8.GetBytes(htm);

            Response.ContentLength64 = buf.Length;

            Stream OutputStream = Response.OutputStream;
            OutputStream.Write(buf, 0, buf.Length);
            OutputStream.Close();
        }

    }
}
