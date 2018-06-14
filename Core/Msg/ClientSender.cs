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

namespace Core
{
    public enum MsgConnectEvent
    {
        CLOSE = 1,
        OPEN = 2,
        MESSAGE_TEXT = 3,
        MESSAGE_BINARY = 4,
        ERROR = 5,
    }

    public class ClientSocket
    { 
        private bool wait = false;
        private bool Sending = false;
        private readonly IMsgQueue<Msg> msg;
        private readonly IWebSocket socket;
        public ClientSocket(ILog _log, IMsgQueue<Msg> _msg, IWebSocket _socket)
        { 
            msg = _msg;
            socket = _socket;
        }

        public void Start()
        {
            new Thread(new ParameterizedThreadStart((o)  => 
            {  
                while (true) 
                {
                    if (wait) continue;
                    Send(); 
                } 
            })).Start(null);
        }

        public void WaitCommit() 
        {
            wait = true;
        }

        public void Commit() 
        {
            wait = false;
        }

        private void Send()
        {
            if (Sending == false)
            {
                if (msg.Count() > 0)
                {
                    Sending = true;
                    Msg m = msg.Dequeue();
                    if (m != null)
                    {
                        byte[] buf = m.Serialize_Msg();
                        if (socket.CheckConnect())
                        {
                            socket.SendAsync(buf, (ok) =>
                            {
                                if (ok)
                                {
                                    Sending = false;
                                }
                            });
                        }
                        else
                            msg.Enqueue(m);
                    }
                }
            }
        }//end send

    }

    public class ClientSender : ISender
    {
        private readonly MsgQueue<Msg> msg;
        private readonly ClientSocket sender;

        public ClientSender(ILog _log, IWebSocket _socket)
        {
            msg = new MsgQueue<Msg>(_log);
            sender = new ClientSocket(_log, msg, _socket);
            sender.Start();
        }
         
        public void Send(Msg m)
        { 
            sender.WaitCommit();
            msg.Enqueue(m);
            sender.Commit();
        }
          
        public void Send(IEnumerable<Msg> list)
        {
            sender.WaitCommit();
            foreach (var m in list)
                msg.Enqueue(m);
            sender.Commit();
        }
    }
}
