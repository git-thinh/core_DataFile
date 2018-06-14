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
using WebSocketSharp;
using Fleck2.Interfaces;

namespace Core
{
    public interface ISender
    {
        void Send(Msg m);
        void Send(IEnumerable<Msg> list); 
    }

    public class HostSocket
    {
        private readonly IMsgQueue<Msg> msg;
        private readonly IWebSocketConnection socket;
        public HostSocket(ILog _log, IMsgQueue<Msg> _msg, IWebSocketConnection _socket)
        {
            msg = _msg;
            socket = _socket;
        }
        public void Send()
        {
            if (msg.Count() > 0)
            {
                Msg m = msg.Dequeue();
                if (m != null)
                {
                    byte[] buf = m.Serialize_Msg();
                    socket.Send(buf);
                    Thread.Sleep(10);
                }
            }
        }
    }

    public class HostSender : ISender
    {
        private readonly MsgQueue<Msg> msg;
        private readonly object _lock = new object();
        private readonly Dictionary<long, IWebSocketConnection> conSocket = new Dictionary<long, IWebSocketConnection>();
        private readonly List<Tuple<long, string>> conName = new List<Tuple<long, string>>();

        private readonly ILog log;

        public HostSender(ILog _log)
        {
            log = _log;
            msg = new MsgQueue<Msg>(_log);

        }

        public void Registry(IWebSocketConnection client)
        {
            long id = client.ConnectionInfo.Id;
            client.Send(BitConverter.GetBytes(id));
            lock (_lock) if (!conSocket.ContainsKey(id)) conSocket.Add(id, client);
            string s = string.Format("Opened websocket client: {0}", id);
            log.Write(LogSystem.HOST_SYSTEM, LogType.USER, s);
            //if (OnClient != null) OnClient(MsgConnectEvent.OPEN, id, 0);

            new Thread(new ParameterizedThreadStart((o) =>
            {
                while (true)
                {
                    HostSocket it = (HostSocket)o;
                    it.Send();
                }
            })).Start(new HostSocket(log, msg, client));
        }

        public void Remove(long connect_id)
        {
            lock (_lock) if (conSocket.ContainsKey(connect_id)) conSocket.Remove(connect_id);
            log.Write(LogSystem.HOST_SYSTEM, LogType.USER, string.Format("Closed websocket client: {0}", connect_id));
            //if (OnClient != null) OnClient(MsgConnectEvent.CLOSE, id, 0);
        }

        public void Send(Msg m)
        {
            msg.Enqueue(m);
        }
         
        public void Send(IEnumerable<Msg> list)
        {
            throw new NotImplementedException();
        }
    }


}
