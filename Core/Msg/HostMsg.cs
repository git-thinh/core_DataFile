using System;
using System.Collections.Generic;
using System.Text;
using Fleck2.Interfaces;

namespace Core
{ 
    public class HostMsg  
    {
        private readonly ILog log;
        private readonly DbStore db;
        private readonly MsgReceiver receiver;
        private readonly HostSender sender;

        public HostMsg(ILog _log) 
        {
            log = _log;

            sender = new HostSender(_log);
            db = new DbStore(_log, sender);
            receiver = new MsgReceiver(_log);
        }

        public void OnOpen(IWebSocketConnection client)
        {
            sender.Registry(client);
        }

        public void OnClose(long connect_id)
        {
            sender.Remove(connect_id);
        }

        public void OnReceiveBinary(byte[] buf) 
        {
            receiver.Receive(buf);
        }
    }
}
