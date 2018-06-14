using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Core
{
    public class MsgQueue 
    {
        private readonly object _lock;
        private readonly Queue<Msg> msg;

        public delegate void OnMessageEvent();
        public event OnMessageEvent OnMessage = null;

        public MsgQueue(ILog _log) 
        {
            _lock = new object();
            msg = new Queue<Msg>(); 
        }

        public Msg Dequeue()
        {
            Msg m = null;
            lock (_lock)
                m = msg.Dequeue();
            return m;
        }

        public void Enqueue(Msg m)
        {
            lock (_lock)
                msg.Enqueue(m);
            if (OnMessage != null) OnMessage();
        }

        public int Count()
        {
            int k = 0;
            lock (_lock)
                k = msg.Count;
            return k;
        }
    }

    public interface IMsgBuffer 
    {
        void Receive(Msg m);
    }

    public class MsgBuffer
    {
        private readonly MsgQueue msg;

        public MsgBuffer(ILog _log, IRequest _db)
        {
            msg = new MsgQueue(_log);
            msg.OnMessage += () => _db.Request(msg.Dequeue());
        }

        public void Receive(Msg m)
        {
            msg.Enqueue(m);
        }

        public void ReceiveNewThread(Msg m)
        {
            new Thread(new ParameterizedThreadStart((x) =>
            { 
                msg.Enqueue(m);
            })).Start(m); 
        }
    }
}
