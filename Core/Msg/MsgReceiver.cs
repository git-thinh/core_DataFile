using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core
{
    public class DbReceiver
    {
        private bool processing = false;
        private readonly IMsgQueue<byte[]> msg; 
        public DbReceiver(ILog _log, IMsgQueue<byte[]> _msg )
        {
            msg = _msg; 
        }

        public void Start() 
        {
            new Thread(new ParameterizedThreadStart((o) =>
            {
                while (true)
                {
                    ProcessMsg();
                }
            })).Start(null);
        }

        private void ProcessMsg()
        {
            //if (processing == false)
            //{
            //    if (msg.Count() > 0)
            //    {
            //        processing = true;
            //        byte[] bf = msg.Dequeue();
            //        if (bf != null)
            //        {
            //            Msg m = bf.Deserialize_Msg();
            //            if (m != null)
            //            {
            //                switch (m.DataType)
            //                {
            //                    case "System.RuntimeType":
            //                        db.Type_AddOrUpdate((Type)m.Data);
            //                        break;
            //                    default:
            //                        switch (m.DataAction)
            //                        {
            //                            case DataAction.DB_ADD:
            //                                break;
            //                        }
            //                        break;
            //                }
            //                processing = false;
            //            }//end msg != null
            //        }
            //    }
            //}
        }
    }

    public class MsgReceiver
    {
        private readonly MsgQueue<byte[]> msg;
        private readonly DbReceiver reciever;

        public MsgReceiver(ILog _log )
        {
            msg = new MsgQueue<byte[]>(_log);
            //reciever = new DbReceiver(_log, msg, _db);
            //reciever.Start();
        }

        public void Receive(byte[] buf)
        {
            msg.Enqueue(buf);
        }
    }














    ////public interface IMsgReceiver
    ////{
    ////    int Count();
    ////    void Add(byte[] buf);
    ////}

    ////public class MsgReceiver : IMsgReceiver
    ////{
    ////    private readonly ReaderWriterLockSlim _lock;
    ////    private readonly Queue<byte[]> msg;

    ////    public event OnChangeEvent OnMsgReceiver = null;
    ////    public delegate void OnChangeEvent(long msg_id);

    ////    private readonly System.Timers.Timer timer;

    ////    private readonly IDB db;

    ////    public MsgReceiver(IDB _db)
    ////    {
    ////        db = _db;

    ////        _lock = new ReaderWriterLockSlim();
    ////        msg = new Queue<byte[]>();
    ////        timer = new System.Timers.Timer(100);
    ////        timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
    ////        timer.Enabled = true;
    ////    }

    ////    // Specify what you want to happen when the Elapsed event is raised.
    ////    private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
    ////    {
    ////        byte[] buf = null;
    ////        using (_lock.ReadLock())
    ////            if (msg.Count > 0)
    ////                buf = msg.Dequeue();
    ////        if (buf != null)
    ////            new Thread(new ParameterizedThreadStart((o) => 
    ////            { 
    ////                byte[] bf = (byte[])o;
    ////                Msg m = bf.Deserialize_Msg();
    ////                if (m != null) 
    ////                {
    ////                    switch (m.DataType) 
    ////                    {
    ////                        case "System.RuntimeType":
    ////                            db.Type_AddOrUpdate((Type)m.Data);
    ////                            break;
    ////                        default:
    ////                            switch (m.DataAction) 
    ////                            {
    ////                                case DataAction.DB_ADD:
    ////                                    break;
    ////                            }
    ////                            break;
    ////                    }
    ////                }
    ////            })).Start(buf);
    ////    }

    ////    public int Count()
    ////    {
    ////        return msg.Count;
    ////    }

    ////    public void Add(byte[] buf)
    ////    {
    ////        if (buf == null || buf.Length == 0) return;
    ////        using (_lock.WriteLock())
    ////            msg.Enqueue(buf);
    ////    }

    ////}
}
