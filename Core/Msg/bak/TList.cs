using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core
{
    public class TList<T>
    {
        private readonly ReaderWriterLockSlim m_lock;
        private readonly List<T> m_store;

        //public event OnLogChangeEvent OnLogChange = null;
        //public delegate void OnLogChangeEvent(string msg, MsgType type);

        public TList()
        {
            m_lock = new ReaderWriterLockSlim();
            m_store = new List<T>();
        }

        public int Count()
        {
            return m_store.Count;
        }

        public T Get(Func<T, bool> match)
        {
            T o = default(T);
            if (m_store.Count > 0)
            {
                m_lock.EnterReadLock();
                try
                {
                    int index = m_store.FindIndex(match);
                    if (index != -1) o = m_store[index];
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
            return o;
        }

        public void Add(T o)
        {
            m_lock.EnterWriteLock();
            try
            {
                m_store.Add(o);
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public void AddOrUpdate(Func<T, bool> match, T val)
        {
            m_lock.EnterWriteLock();
            try
            {
                int index = m_store.FindIndex(match);
                if (index == -1)
                    m_store.Add(val);
                else
                    m_store[index] = val;
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        public void Remove(Func<T, bool> match)
        {
            if (m_store.Count > 0)
            {
                m_lock.EnterWriteLock();
                try
                {
                    int index = m_store.FindIndex(match);
                    if (index != -1) m_store.RemoveAt(index);
                }
                finally
                {
                    m_lock.ExitWriteLock();
                }
            }
        }
    }
}
