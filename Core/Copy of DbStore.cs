using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Linq;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

namespace Core
{
    [Serializable]
    public class ItemID 
    {
        public int Index { set; get; }
        public int Id { set; get; }
        public ItemID() {
            Index = -1;
            Id = -1;
        }
        public ItemID(int _index, int _id) 
        {
            Index = _index;
            Id = _id;
        }
    }

    public class TypeIndex
    {
        public Type Type { set; get; }
        public int Index { set; get; }

        public TypeIndex() 
        {
            Index = -1;
        }

        public TypeIndex(Type _type, int _index) : this()
        {
            Type = _type;
            Index = _index;
        }
    }

    public interface IRequest
    {
        void Request(Msg m);
    }

    public class DbStore : IRequest
    {
        private readonly ILog log;

        private readonly object _lockType;
        private readonly List<string> listType;
        private readonly Dictionary<string, Type> storeType;

        private readonly object _lockRW;
        private readonly Dictionary<int, ReaderWriterLockSlim> storeLock;
        private readonly Dictionary<int, IList> storeData;

        private readonly ISender sender;
        private readonly JsonSerializer serializer;

        public DbStore(ILog _log, ISender _sender)
            : this(_log)
        {
            sender = _sender;
        }

        public DbStore(ILog _log)
        {
            log = _log;
            serializer = new JsonSerializer();

            listType = new List<string>();

            _lockType = new object();
            storeType = new Dictionary<string, Type>();

            _lockRW = new object();
            storeLock = new Dictionary<int, ReaderWriterLockSlim>();
            storeData = new Dictionary<int, IList>();
        }
        
        public void Request(Msg m)
        {
            switch (m.DataAction)
            {
                case DataAction.DB_ADD:
                    #region [ === DB_ADD === ]
                    try
                    {
                        TypeIndex ti = type_Get(m.DataType);
                        if (ti != null && ti.Index != -1)
                        {
                            object val; 
                            using (MemoryStream ms = new MemoryStream(m.DataBuffer))
                            using (BsonReader reader = new BsonReader(ms)) 
                                val = serializer.Deserialize(reader, ti.Type);
                            Item_AddOrUpdate(ti.Index, val);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    break;
                    #endregion
                case DataAction.DB_REG_MODEL:
                    #region [ === DB_REG_MODEL === ]
                    try
                    {
                        Model mo = (Model)m.Data;
                        string src = mo.ToString();

                        string key = mo.Namespace + "." + mo.ClassName;
                        int index = type_GetIndex(key);
                        if (index != -1) return;

                        CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
                        //CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");  
                        CompilerParameters parameter = new CompilerParameters();
                        // True - memory generation, false - external file generation
                        parameter.GenerateInMemory = true;
                        // True - exe file generation, false - dll file generation
                        parameter.GenerateExecutable = false;
                        parameter.ReferencedAssemblies.Add(@"System.dll");
                        parameter.IncludeDebugInformation = false;

                        CompilerResults result = provider.CompileAssemblyFromSource(parameter, src);
                        if (result.Errors.HasErrors)
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (CompilerError error in result.Errors)
                                sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                            string err = sb.ToString();
                        }
                        else
                        {
                            Assembly asm = result.CompiledAssembly;
                            //string[] aName = asm.GetTypes().Select(x => x.FullName).ToArray();
                            //modelUser = asm.GetType(aName[0], false);
                            Type type = asm.GetTypes()[0];
                            type_AddOrUpdate(type);
                        }
                    }
                    catch (Exception ex) 
                    {
                    
                    }
                    break;
                    #endregion
                default:
                    break;
            }
        }



        #region [ === TYPE === ]

        private int type_GetIndex(string type_name)
        {
            int index = -1;
            if (string.IsNullOrEmpty(type_name)) return index;
            lock (_lockType)
                index = listType.IndexOf(type_name);
            return index;
        }

        private int type_AddOrUpdate(Type type)
        {
            if (type == null) return -1;
            string key = type.FullName;

            int index = -1;
            lock (_lockType)
            {
                if (storeType.ContainsKey(key))
                    storeType[type.FullName] = type;
                else
                {
                    storeType.Add(key, type);
                    listType.Add(key);
                    index = listType.Count - 1;
                }
            }
            if (index >= 0)
            {
                lock (_lockRW)
                {
                    storeLock.Add(index, new ReaderWriterLockSlim());

                    var listType = typeof(List<>);
                    var constructedListType = listType.MakeGenericType(type);
                    var instance = Activator.CreateInstance(constructedListType);
                    IList list = (IList)instance;

                    storeData.Add(index, list);
                }
            }

            return index;
        }

        private TypeIndex type_Get(string type_Name)
        {
            if (type_Name == null) return null;
            int index = type_GetIndex(type_Name);
            Type type;
            lock (_lockType)
                storeType.TryGetValue(type_Name, out type);
            return new TypeIndex(type, index);
        }

        #endregion

        #region [ === ITEM === ]

        private bool Item_Remove(object item)
        {
            int index = -1;
            if (item == null) return false;

            Type type = item.GetType();
            string key = type.FullName;

            index = type_GetIndex(key);
            if (index == -1)
                index = type_AddOrUpdate(type);
            else
            {
                IList list = null;
                ReaderWriterLockSlim rw = null;

                lock (_lockRW)
                    storeLock.TryGetValue(index, out rw);

                if (rw != null)
                {
                    using (rw.WriteLock())
                    {
                        if (storeData.TryGetValue(index, out list))
                        {
                            int index_it = list.IndexOf(item);
                            if (index_it != -1)
                            {
                                list[index_it] = null;
                                storeData[index] = list;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private ItemID Item_AddOrUpdate(int index, object item)
        {
            if (index == -1) return new ItemID();

            int id = -1;
            IList list = null;
            ReaderWriterLockSlim rw = null;

            lock (_lockRW)
                storeLock.TryGetValue(index, out rw);

            if (rw != null)
            {
                using (rw.WriteLock())
                {
                    if (storeData.TryGetValue(index, out list))
                    {
                        list.Add(item);
                        storeData[index] = list;
                        id = list.Count - 1;
                    }
                }
            }

            return new ItemID(index, id);
        }
        
        private ItemID Item_AddOrUpdate(object item)
        { 
            if (item == null) return new ItemID();

            Type type = item.GetType();
            string key = type.FullName;

            int index = type_GetIndex(key);
            if (index >= 0)
                return Item_AddOrUpdate(index, item);

            return new ItemID();
        }

        #endregion
    }
}
