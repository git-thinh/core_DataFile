using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
////using model;

//////namespace model
//////{
//////    [Serializable]
//////    public class mpUser
//////    {
//////        public string Username { get; set; }
//////        public string Password { get; set; }
//////        public string Fullname { get; set; }
//////    }
//////}
namespace Core
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class ClientConnect : IMsg
    {
        private int PortHost = 10101;

        private readonly DbStore db;
        private readonly ClientSender sender;
        private readonly MsgReceiver receiver;

        public delegate void OnMessageEvent(MsgConnectEvent TypeEvent, object Data);
        public event OnMessageEvent OnMessage;

        public void ProcessMsg(byte[] buf)
        {

        }

        private readonly WebSocketSharp.WebSocket socket;
        public ClientConnect(ILog _log)
        {
            receiver = new MsgReceiver(_log);
            socket = new WebSocketSharp.WebSocket(PortHost, "msg");

            sender = new ClientSender(_log, socket);
            db = new DbStore(_log, sender); 
            ///////////////////////////////////////////////////////////////////////
            //   
            socket.OnOpen += (se, e) =>
            { 
                //////sender.Send(
                //////    new Msg[]{
                //////        new Msg() { Data = typeof(mpUser), },
                //////        new Msg() { DataAction = DataAction.DB_ADD,
                //////                    Data = new mpUser() { Username = "thinh", Password = "12345", Fullname = "Nguyễn Văn Thịnh" } },
                //////        new Msg() { DataAction = DataAction.DB_ADD,
                //////                    Data = new mpUser() { Username = "tu", Password = "99999", Fullname = "Nguyễn Cam Tu" }
                //////     }
                //////}); 
                //if (OnMessage != null) OnMessage(MsgConnectEvent.OPEN, null);
            };
            socket.OnMessage += (se, e) =>
            {
                //////////////////////////////////////////
                // REGISTRY SOCKET ID
                if (socket.ID == 0 && e.IsBinary)
                {
                    try
                    {
                        long id = BitConverter.ToInt64(e.RawData, 0);
                        if (id > 0) socket.ID = id;
                    }
                    catch { }
                    return;
                }

                //////////////////////////////////////////
                // REGISTRY SOCKET ID 
                if (e.IsBinary) 
                    receiver.Receive(e.RawData);
            };
            socket.OnError += (se, e) =>
            {
                if (OnMessage != null) OnMessage(MsgConnectEvent.ERROR, null);
            };
            socket.OnClose += (se, e) =>
            {
                if (OnMessage != null) OnMessage(MsgConnectEvent.CLOSE, null);
            };
        }

        public void Start()
        {
            socket.Connect();
        }

        public void Close()
        {
            socket.Close();
        }

        public void Send(Msg m)
        {
            sender.Send(m);
        }

        //public void SendText(string text)
        //{
        //    socket.Send(text);
        //}

        //public void Send(object data)
        //{
        //    Msg m = new Msg()
        //    { 
        //        Data = data,
        //    };

        //    byte[] buf;
        //    using (var ms = new MemoryStream())
        //    {
        //        Serializer.NonGeneric.Serialize(ms, m);
        //        buf = ms.ToArray();
        //    }
        //    socket.Send(buf);
        //}

        //public T Send<T>(object data)
        //{
        //    T val;
        //    using (var client = new WebClient() { BaseAddress = "http://127.0.0.1:" + PortHost.ToString() })
        //    {
        //        byte[] buf;
        //        using (var ms = new MemoryStream())
        //        {
        //            Serializer.NonGeneric.Serialize(ms, data);
        //            buf = ms.ToArray();
        //        }
        //        buf = client.UploadData("/", buf); // data is now the response
        //        using (var ms = new MemoryStream(buf))
        //            val = Serializer.Deserialize<T>(ms);
        //    }
        //    return val;
        //}

        //public T Get<T>(string key)
        //{
        //    T val;
        //    using (var client = new WebClient() { BaseAddress = "http://127.0.0.1:" + PortHost.ToString() })
        //    {
        //        byte[] buf = client.DownloadData("/" + key);
        //        using (var ms = new MemoryStream(buf))
        //            val = Serializer.Deserialize<T>(ms);
        //    }
        //    return val;
        //}

    }//end class 
}
