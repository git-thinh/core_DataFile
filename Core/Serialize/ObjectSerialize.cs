using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System;
using ProtoBuf;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

namespace Core
{
    public class ResultDeserialize
    {
        public bool OK { set; get; }
        public object Data { set; get; }
        public string DataType { set; get; }

        public ResultDeserialize(bool ok, object data, string type_Data)
        {
            OK = ok;
            Data = data;
            DataType = type_Data;
        }
    }

    public static class ObjectSerialize
    {
        public static byte[] Serialize_Object(this object o)
        {
            byte[] buf = null;
            if (o == null) return buf;
            string datatype = o.GetType().FullName;

            string code = datatype.Split('`')[0];
            switch (code)
            {
                case "System.String":
                    buf = Encoding.UTF8.GetBytes(o.ToString());
                    break;
                case "<>f__AnonymousType0":
                    #region
                    using (var ms = new MemoryStream())
                    {
                        var formatter = new BinaryFormatter();
                        using (var ds = new DeflateStream(ms, CompressionMode.Compress, true))
                            formatter.Serialize(ds, o.AsDictionary());
                        ms.Position = 0;
                        buf = ms.GetBuffer();
                    }
                    break;
                    #endregion
                case "System.Linq.QueryableEnumerable":
                    #region [ Queryable Enumerable ]

                    List<IDictionary<string, object>> ls = ((IQueryable)o).ToListDictionary();
                    using (var ms = new MemoryStream())
                    {
                        var formatter = new BinaryFormatter();
                        using (var ds = new DeflateStream(ms, CompressionMode.Compress, true))
                            formatter.Serialize(ds, ls);
                        ms.Position = 0;
                        buf = ms.GetBuffer();
                    }
                    break;

                    #endregion
                case "System.RuntimeType":
                    #region [ === Type === ]
                    try
                    {
                        Type type = (Type)o;
                        var fs = type.GetProperties()
                            .Select(x => new Field() { Name = x.Name, Type = x.PropertyType.Name })
                            .ToArray();
                        Model mo = new Model()
                        {
                            Namespace = type.Namespace,
                            ClassName = type.Name,
                            Fields = fs,
                        };

                        using (var stream = new MemoryStream())
                        {
                            Serializer.NonGeneric.Serialize(stream, mo);
                            //Serializer.Serialize(stream, Data);
                            buf = stream.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    break;
                    #endregion
                case "System.Func":
                    #region [ === FUNC === ]
                    try
                    {
                        ////using (var ms = new MemoryStream())
                        ////using (var writer = new BsonWriter(ms))
                        ////{
                        ////    var serializer = new JsonSerializer();
                        ////    serializer.Serialize(writer, o);
                        ////    buf = ms.ToArray();
                        ////}
                        using (MemoryStream ms = new MemoryStream())
                        {
                            IFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(ms, o);
                            buf = ms.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    break;
                    #endregion
                default:
                    #region [ === BSON === ]

                    try
                    {
                        using (var ms = new MemoryStream())
                        using (var writer = new BsonWriter(ms))
                        {
                            var serializer = new JsonSerializer();
                            serializer.Serialize(writer, o);
                            buf = ms.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    break;

                ////using (MemoryStream ms = new MemoryStream())
                ////{
                ////    IFormatter formatter = new BinaryFormatter();
                ////    formatter.Serialize(ms, o);
                ////    buf = ms.ToArray();
                ////}

                ////using (var stream = new MemoryStream())
                ////{
                ////    Serializer.NonGeneric.Serialize(stream, o);
                ////    //Serializer.Serialize(stream, Data);
                ////    buf = stream.ToArray(); //GetBuffer was giving me a Protobuf.ProtoException of "Invalid field in source data: 0" when deserializing
                ////}

                //byte[] buf22 = System.Binary.BinarySerializer.Serialize(o);

                //using (var ms = new System.IO.MemoryStream())
                //{
                //    using (var gZipStream = new GZipStream(ms, CompressionMode.Compress))
                //    {
                //        BinaryFormatter binaryFormatter = new BinaryFormatter();
                //        binaryFormatter.Serialize(gZipStream, o);
                //    }
                //    buf = ms.ToArray();
                //} 

                ////using (var ms = new System.IO.MemoryStream())
                ////{
                ////    using (DeflateStream deflateStream = new DeflateStream(ms, CompressionMode.Compress))
                ////        new BinaryFormatter().Serialize(deflateStream, o);
                ////    buf = ms.ToArray();
                ////}

                //////int size = Marshal.SizeOf(o);
                //////buf = new byte[size];
                //////IntPtr p = Marshal.AllocHGlobal(size);
                //////try
                //////{
                //////    Marshal.StructureToPtr(o, p, false);
                //////    Marshal.Copy(p, buf, 0, size);
                //////}
                //////finally
                //////{
                //////    Marshal.FreeHGlobal(p);
                //////}

                //string jsonString = JsonConvert.SerializeObject(o);
                ////var jsonObj = JsonConvert.DeserializeObject(jsonString);

                ////using(MemoryStream ms = new MemoryStream())
                ////using (BsonWriter writer = new BsonWriter(ms))
                ////{
                ////    JsonSerializer serializer = new JsonSerializer();
                ////    serializer.Serialize(writer, jsonObj);
                ////    buf = ms.ToArray();
                ////}

                //buf = Encoding.UTF8.GetBytes(jsonString);

                    #endregion
            }
            return buf;
        }

        public static ResultDeserialize Deserialize_Object(this byte[] buf, string type_Data)
        {
            object val = null;
            bool ok = false;
            if (buf == null || buf.Length == 0 || string.IsNullOrEmpty(type_Data)) return new ResultDeserialize(ok, val, type_Data);

            string code = type_Data.Split('`')[0];
            switch (code)
            {
                case "System.String":
                    val = Encoding.UTF8.GetString(buf);
                    ok = true;
                    break;
                case "<>f__AnonymousType0":
                    #region
                    using (var ms = new MemoryStream(buf))
                    {
                        var formatter = new BinaryFormatter();
                        using (var ds = new DeflateStream(ms, CompressionMode.Decompress, true))
                        {
                            val = (IDictionary<string, object>)formatter.Deserialize(ds);
                            ok = true;
                        }
                    }
                    break;
                    #endregion
                case "System.Linq.QueryableEnumerable":
                    #region [ Queryable Enumerable ]

                    using (var ms = new MemoryStream(buf))
                    {
                        var formatter = new BinaryFormatter();
                        using (var ds = new DeflateStream(ms, CompressionMode.Decompress, true))
                        {
                            val = (List<IDictionary<string, object>>)formatter.Deserialize(ds);
                            ok = true;
                        }
                    }
                    break;
                    #endregion
                case "System.RuntimeType":
                    #region [ === Type === ]
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(buf))
                        {
                            //IFormatter formatter = new BinaryFormatter();
                            //val = formatter.Deserialize(ms);
                            val = Serializer.Deserialize<Model>(ms);
                            ok = true;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    break;

                    #endregion
                case "System.Func":
                    #region [ === FUNC === ]
                    try
                    {

                        using (var ms = new MemoryStream(buf)) 
                        {
                            IFormatter formatter = new BinaryFormatter();
                            val = formatter.Deserialize(ms);
                            ok = true;
                        }

                    }
                    catch (Exception ex) 
                    {
                    
                    }
                    break;

                    #endregion
                default:
                    #region [ === BSON === ]

                    try
                    {
                        Type type = DbType.Get(type_Data);
                        if (type != null)
                        {
                            using (MemoryStream ms = new MemoryStream(buf))
                            using (BsonReader reader = new BsonReader(ms))
                            {
                                var serializer = new JsonSerializer();
                                val = serializer.Deserialize(reader, type);
                                ok = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ok = false;
                    }
                    break;

                ////Type type = Type.GetType(type_Data);
                ////if (type != null)
                ////    using (var ms = new MemoryStream(buf))
                ////    {
                ////        val = Serializer.NonGeneric.Deserialize(type, ms);
                ////        ok = true;
                ////    }

                ////Type type = Type.GetType(type_Data);
                ////if (type != null)
                ////    using (var stream = new MemoryStream(buf))
                ////    {
                ////        //val = System.Binary.BinarySerializer.Deserialize(type, stream);
                ////        //ok = true;
                ////    }

                //////using (var ms = new MemoryStream(b))
                //////{
                //////    using (var decompressor = new GZipStream(ms, CompressionMode.Decompress))
                //////    { 
                //////        val = new BinaryFormatter().Deserialize(decompressor);
                //////        ok = true;
                //////    }
                //////} 

                ////using (var ms = new MemoryStream(buf))
                ////{
                ////    using (DeflateStream deflateStream = new DeflateStream(ms, CompressionMode.Decompress))
                ////    {
                ////        val = new BinaryFormatter().Deserialize(deflateStream);
                ////        ok = true;
                ////    }
                ////} 

                    #endregion
            }

            return new ResultDeserialize(ok, val, type_Data);
        }
    }

}
