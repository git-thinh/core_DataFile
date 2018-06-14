//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Collections.Generic
{
    using IO;
    using Reflection;
    using Runtime.Serialization;
    using Runtime.Serialization.Formatters.Binary;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    [System.Runtime.InteropServices.ComVisible(false)]
    public class SynchronizedDictionaryList<TKey, TValue>
    {
        private static Dictionary<TKey, List<TValue>> db = new Dictionary<TKey, List<TValue>>() { };
        private static readonly object _lock = new object();

        public List<TValue> this[TKey key]
        {
            get
            {
                List<TValue> v;
                db.TryGetValue(key, out v);
                return v;
            }

            set
            {
                lock (_lock)
                    if (db.ContainsKey(key))
                        db[key] = value;
                    else
                        db.Add(key, value);
            }
        }

        public int Count
        {
            get
            {
                return db.Count;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return db.Keys;
            }
        }

        public ICollection<List<TValue>> Values
        {
            get
            {
                return db.Values;
            }
        }

        public void AddToTop_ItemAll(TValue value)
        {
            lock (_lock)
            {
                foreach (TKey key in db.Keys)
                {
                    List<TValue> v = new List<TValue>() { };
                    if (db.TryGetValue(key, out v))
                    {
                        v.Insert(0, value);
                        db[key] = v;
                    }
                    else
                    {
                        v.Add(value);
                        db[key] = v;
                    }
                }
            }
        }

        public void AddToTop_Item(TKey key, TValue value)
        {
            List<TValue> v = new List<TValue>() { };
            if (db.TryGetValue(key, out v))
            {
                v.Insert(0, value);
                lock (_lock)
                    db[key] = v;
            }
            else
            {
                v = new List<TValue>() { value };
                lock (_lock)
                    db.Add(key, v);
            }
        }

        public TValue GetAndRemoveItemTop(TKey key, TValue val_default = default(TValue))
        {
            List<TValue> v = new List<TValue>() { };
            if (db.TryGetValue(key, out v))
            {
                if (v.Count > 0)
                {
                    val_default = Clone(v[0]);
                    lock (_lock)
                    {
                        v.RemoveAt(0);
                        db[key] = v;
                    }
                }
            }
            return val_default;
        }

        public TValue GetAndRemoveItemLast(TKey key, TValue val_default = default(TValue))
        {
            List<TValue> v = new List<TValue>() { };
            if (db.TryGetValue(key, out v))
            {
                if (v.Count > 0)
                {
                    val_default = Clone(v[v.Count - 1]);
                    if (v.Count > 0)
                    {
                        lock (_lock)
                        {
                            v.RemoveAt(v.Count - 1);
                            db[key] = v;
                        }
                    }
                }
            }
            return val_default;
        }


        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        private static TValue Clone(TValue source)
        {
            if (!typeof(TValue).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(TValue);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (TValue)formatter.Deserialize(stream);
            }
        }
    }

    public class SynchronizedQuereString
    {
        const int _keyNull = -1;
        private static List<int> listKeyFree = new List<int>() { };
        private static List<int> listKey = new List<int>() { };
        private static Dictionary<int, string> db = new Dictionary<int, string>() { };
        private static readonly object _lock = new object();
         
        public int Count
        {
            get
            {
                return listKey.Count;
            }
        }

        public int Update(string value)
        {
            int k = _keyNull;
            lock (_lock)
            {
                int key = _keyNull;
                if (listKeyFree.Count > 0)
                {
                    key = listKeyFree[0];
                    listKeyFree.RemoveAt(0);
                }
                else if (listKey.Count == 0)
                {
                    key = 1;
                }
                else {
                    key = listKey.Max() + 1;
                }

                db.Add(key, value);
                listKey.Add(key);
                k = listKey.Count;
            }
            return k;
        }

        public string get_Item_Last_And_Remove()
        {
            string val = string.Empty;
            if (listKey.Count > 0)
            {
                lock (_lock)
                {
                    int key = listKey[0];
                    val = db[key];
                    listKey.RemoveAt(0);
                    listKeyFree.Add(key);
                    db.Remove(key);
                } 
            }
            return val;
        }
    }

    [System.Runtime.InteropServices.ComVisible(false)]
    public class SynchronizedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        private static Dictionary<TKey, TValue> db = new Dictionary<TKey, TValue>() { };
        private static readonly object _lock = new object();


        public TValue[] All()
        {
            TValue[] a = new TValue[] { };
            lock (_lock)
                a = db.Values.ToArray();
            return a;
        }

        public TValue[] Query(Func<TValue, bool> where)
        {
            TValue[] a = new TValue[] { };
            lock (_lock)
                a = db.Values.Where(where).ToArray();
            return a;
        }

        /// <summary>
        /// Update or Insert item </key>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                TValue v;
                db.TryGetValue((TKey)key, out v);
                return v;
            }

            set
            {
                lock (_lock)
                    if (db.ContainsKey((TKey)key))
                        db[(TKey)key] = (TValue)value;
                    else
                        db.Add((TKey)key, (TValue)value);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue v;
                db.TryGetValue(key, out v);
                return v;
            }

            set
            {
                lock (_lock)
                    if (db.ContainsKey(key))
                        db[key] = value;
                    else
                        db.Add(key, value);
            }
        }

        public int Count
        {
            get
            {
                return db.Count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return db.Keys;
            }
        }

        public object SyncRoot
        {
            get
            {
                return false;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return db.Values;
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return db.Keys;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return db.Values;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_lock)
                if (db.ContainsKey(item.Key))
                    db[item.Key] = item.Value;
                else
                    db.Add(item.Key, item.Value);
        }

        public void Add(object key, object value)
        {
            try
            {
                lock (_lock)
                    if (db.ContainsKey((TKey)key))
                        db[(TKey)key] = (TValue)value;
                    else
                        db.Add((TKey)key, (TValue)value);
            }
            catch { }
        }


        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (db.ContainsKey(key))
                    db[key] = value;
                else
                    db.Add(key, value);
            }
        }

        public int AddReturn(TKey key, TValue value)
        {
            int k = 0;
            lock (_lock)
            {
                if (db.ContainsKey(key))
                    db[key] = value;
                else
                    db.Add(key, value);
                k = db.Count;
            }

            return k;
        }

        public void AddArray(string nameKey, TValue[] a)
        {
            Type ti = typeof(TValue);
            PropertyInfo pr = ti.GetProperty(nameKey);
            if (pr != null)
            {
                for (int k = 0; k < a.Length; k++)
                {
                    TValue o = a[k];
                    var id = pr.GetValue(o, null);
                    if (id != null)
                    {
                        try
                        {
                            TKey key = (TKey)id;
                            if (!db.ContainsKey(key))
                                db.Add(key, o);
                        }
                        catch { }
                    }
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
                db.Clear();
        }

        public bool Contains(object key)
        {
            return db.ContainsKey((TKey)key);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return db.ContainsKey(item.Key);
        }

        public bool ContainsKey(TKey key)
        {
            return db.ContainsKey(key);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Remove(object key)
        {
            lock (_lock)
                if (db.ContainsKey((TKey)key))
                    db.Remove((TKey)key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_lock)
                if (db.ContainsKey(item.Key))
                {
                    db.Remove(item.Key);
                    return true;
                }
            return false;
        }

        public bool Remove(TKey key)
        {
            lock (_lock)
                if (db.ContainsKey(key))
                {
                    db.Remove(key);
                    return true;
                }
            return false;
        }


        public bool TryGetValue(TKey key, out TValue value)
        {
            return db.TryGetValue(key, out value);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
