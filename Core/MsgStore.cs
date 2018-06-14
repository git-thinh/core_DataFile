using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core
{
    public class MsgStore : IMsgLog
    {
        private readonly object _lock;
        private readonly List<string> m_log;
        private readonly ReaderWriterLockSlim m_lockStoreData;
        private readonly IDictionary<string, object> m_storeData;
        private readonly IDictionary<string, string> m_storeDataString;

        public event OnLogChangeEvent OnLogChange = null;
        public delegate void OnLogChangeEvent(string msg, MsgType type);

        private readonly bool isDebuger = false;

        public MsgStore(bool debug = false)
        {
            isDebuger = debug;

            _lock = new object();
            m_log = new List<string>();

            m_lockStoreData = new ReaderWriterLockSlim();
            m_storeData = new Dictionary<string, object>();
            m_storeDataString = new Dictionary<string, string>();
        }

        public void LogWrite(string text, MsgType type)
        {
            LogWrite(text, type, null);
        }

        public void LogWrite(string text, MsgType type, string subfix)
        {
            if (type == MsgType.FINISH)
                text = "#" + ((int)type).ToString() + " " + text;
            else
            {
                if (!string.IsNullOrEmpty(subfix))
                {
                    if (subfix.Contains("{0}"))
                        text = string.Format(subfix, text);
                    else
                        text = subfix + " " + text;
                }

                if (type != MsgType.SYSTEM)
                    text = type.ToString() + ": " + text;

                text = "#" + ((int)type).ToString() + " " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff ") + text;
            }

            lock (_lock)
                m_log.Add(text);

            if (OnLogChange != null) OnLogChange(text, type);
        }

        public void LogClear() { lock (_lock) m_log.Clear(); }

        public string LogGet(MsgType type, string hr)
        {
            if (string.IsNullOrEmpty(hr)) hr = Environment.NewLine;

            string s = "";
            if (type != MsgType.SYSTEM)
            {
                lock (_lock)
                {
                    var a = m_log.Where(x => x.Contains(" " + type.ToString() + ": ")).ToArray();
                    s = string.Join(hr, a);
                }
            }
            else
                lock (_lock)
                    s = string.Join(hr, m_log.ToArray());

            return s;
        }

        public int Count()
        {
            return m_storeData.Count;
        }

        public object GetObject(string key)
        {
            if (m_storeData.Count > 0)
            {
                m_lockStoreData.EnterReadLock();
                try
                {
                    object val;
                    if (m_storeData.TryGetValue(key, out val))
                        return val;
                }
                finally
                {
                    m_lockStoreData.ExitReadLock();
                }
            }
            return null;
        }

        public string GetStringData(string key)
        {
            if (m_storeDataString.Count > 0)
            {
                m_lockStoreData.EnterReadLock();
                try
                {
                    string val;
                    if (m_storeDataString.TryGetValue(key, out val))
                        return val;
                }
                finally
                {
                    m_lockStoreData.ExitReadLock();
                }
            }
            return null;
        }

        public void UpdateStringData(string key, string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            m_lockStoreData.EnterWriteLock();
            try
            {
                string val;
                if (m_storeDataString.TryGetValue(key, out val))
                {
                    if (string.IsNullOrEmpty(val)) val = text; else val += "," + text;
                    m_storeDataString[key] = val;
                }
                else
                {
                    m_storeDataString.Add(key, text);
                }
            }
            finally
            {
                m_lockStoreData.ExitWriteLock();
            }
        }

        public object SerializeData(string key)
        {
            ////m_lockStoreData.EnterWriteLock();
            ////try
            ////{
            ////    string text;
            ////    if (m_storeDataString.TryGetValue(key, out text) && !string.IsNullOrEmpty(text))
            ////    {
            ////        text = "[" + text + "]";
            ////        try
            ////        {
            ////            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>[]>(text);

            ////            object val;
            ////            if (m_storeData.TryGetValue(key, out val))
            ////            {
            ////                val.Data = data;
            ////                m_storeData[key] = val;
            ////            }
            ////            else
            ////            {
            ////                val = new object() { Name = key, Data = data };
            ////                m_storeData.Add(key, val);
            ////            }

            ////            return val;
            ////        }
            ////        catch (Exception ex)
            ////        {

            ////        }
            ////    }
            ////}
            ////finally
            ////{
            ////    m_lockStoreData.ExitWriteLock();
            ////}

            return null;
        }

        public void AddOrUpdateHeader(string key, Dictionary<string, object> header)
        {
            ////if (header == null || header.Count == 0) return;

            ////m_lockStoreData.EnterWriteLock();
            ////try
            ////{
            ////    object val;
            ////    if (m_storeData.TryGetValue(key, out val))
            ////    {
            ////        Dictionary<string, object> head = val.Header;

            ////        if (head == null || head.Count == 0)
            ////            val.Header = header;
            ////        else
            ////        {
            ////            string[] akey = header.Keys.ToArray();
            ////            for (int k = 0; k < header.Count; k++)
            ////            {
            ////                if (head.ContainsKey(akey[k]))
            ////                    head[akey[k]] = header[akey[k]];
            ////                else
            ////                    head.Add(akey[k], header[akey[k]]);
            ////            }
            ////            val.Header = head;
            ////        }
            ////        m_storeData[key] = val;
            ////    }
            ////    else
            ////    {
            ////        val = new object() { Name = key, Header = header };
            ////        m_storeData.Add(key, val);
            ////    }
            ////}
            ////finally
            ////{
            ////    m_lockStoreData.ExitWriteLock();
            ////}
        }

        public void Delete(string key)
        {
            m_lockStoreData.EnterWriteLock();
            try
            {
                m_storeDataString.Remove(key);
                m_storeData.Remove(key);
            }
            finally
            {
                m_lockStoreData.ExitWriteLock();
            }
        }
    }
}
