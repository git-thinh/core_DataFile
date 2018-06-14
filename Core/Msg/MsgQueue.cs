using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public interface IMsgQueue<T>
    {
        int Count();
        T Dequeue();
        void Enqueue(T m);
    }

    public class MsgQueue<T> : IMsgQueue<T>
    {
        private readonly object _lock = new object();
        private readonly Queue<T> msg = new Queue<T>();

        public MsgQueue(ILog _log) { }

        public T Dequeue()
        {
            T m = default(T);
            lock (_lock)
                m = msg.Dequeue();
            return m;
        }

        public void Enqueue(T m)
        {
            lock (_lock)
                msg.Enqueue(m);
        }

        public int Count()
        {
            int k = 0;
            lock (_lock)
                k = msg.Count;
            return k;
        }
    }
}
