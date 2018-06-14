using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
namespace Core
{
    public static class MsgSerialize
    {
        public static byte[] Serialize_Msg(this Msg m)
        {
            byte[] buf = null;
            try
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    ProtoBuf.Serializer.NonGeneric.Serialize(ms, m);
                    buf = ms.ToArray();
                }
            }
            catch { }
            return buf;
        }

        public static Msg Deserialize_Msg(this Stream stream)
        {
            Msg m = null;
            try
            {
                m = ProtoBuf.Serializer.Deserialize<Msg>(stream);
            }
            catch { }
            return m;
        }

        public static Msg Deserialize_Msg(this byte[] buf)
        {
            Msg m = null;
            try
            {
                using (var ms = new System.IO.MemoryStream(buf))
                    m = ProtoBuf.Serializer.Deserialize<Msg>(ms);
            }
            catch { }
            return m;
        }// end function


        public static void PostToURL(this Msg m, string uri)
        {
            byte[] buf = m.Serialize_Msg();

            //string uri = "http://127.0.0.1:10101/";
            WebRequest request = WebRequest.Create(uri);
            request.Method = "POST";

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = buf.Length;

            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = 0;

            Stream streamPUT = request.GetRequestStream();
            streamPUT.Write(buf, 0, buf.Length);
            streamPUT.Close();

            WebResponse response = request.GetResponse();
            //if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK)
            //{
            //    string status = ((HttpWebResponse)response).StatusDescription;
            //    if (status == "OK")
            //    {
            //        Console.WriteLine("OK");
            //    }
            //}
            response.Close();
        }



        //////public static object ToNonAnonymousList<T>(this List<T> list, Type t)
        //////{
        //////    //define system Type representing List of objects of T type:
        //////    Type genericType = typeof(List<>).MakeGenericType(t);

        //////    //create an object instance of defined type:
        //////    object l = Activator.CreateInstance(genericType);

        //////    //get method Add from from the list:
        //////    MethodInfo addMethod = l.GetType().GetMethod("Add");

        //////    //loop through the calling list:
        //////    foreach (T item in list)
        //////    {
        //////        //convert each object of the list into T object by calling extension ToType<T>()
        //////        //Add this object to newly created list:
        //////        addMethod.Invoke(l, new[] { item.ToType(t) });
        //////    }
        //////    //return List of T objects:
        //////    return l;
        //////}

        //////public static object ToType<T>(this object obj, T type)
        //////{
        //////    //create instance of T type object:
        //////    object tmp = Activator.CreateInstance(Type.GetType(type.ToString()));

        //////    //loop through the properties of the object you want to covert:          
        //////    foreach (PropertyInfo pi in obj.GetType().GetProperties())
        //////    {
        //////        try
        //////        {
        //////            //get the value of property and try to assign it to the property of T type object:
        //////            tmp.GetType().GetProperty(pi.Name).SetValue(tmp, pi.GetValue(obj, null), null);
        //////        }
        //////        catch (Exception ex)
        //////        {
        //////            //Logging.Log.Error(ex);
        //////        }
        //////    }
        //////    //return the T type object:         
        //////    return tmp;
        //////}


        ////// for IQueryable
        ////public static IList ToNonAnonymousList(this IQueryable source, Type type)
        ////{
        ////    if (source == null) throw new ArgumentNullException("source");

        ////    var returnList = (IList)typeof(List<>)
        ////        .MakeGenericType(source.ElementType)
        ////        .GetConstructor(Type.EmptyTypes)
        ////        .Invoke(null);

        ////    foreach (var elem in source)
        ////        returnList.Add(elem);

        ////    return returnList;
        ////}

        public static object ToNonAnonymousList(this IQueryable source, Type type)
        {
            //define system Type representing List of objects of T type:
            Type genericType = typeof(List<>).MakeGenericType(type);

            //create an object instance of defined type:
            object l = Activator.CreateInstance(genericType);

            var returnList = (IList)typeof(List<>)
                .MakeGenericType(type)
                .GetConstructor(Type.EmptyTypes)
                .Invoke(null);

            ////////get method Add from from the list:
            //////MethodInfo addMethod = l.GetType().GetMethod("Add");

            ////////loop through the calling list:
            //////foreach (var item in list)
            //////{
            //////    //convert each object of the list into T object by calling extension ToType<T>()
            //////    //Add this object to newly created list:
            //////    addMethod.Invoke(l, new[] { item.ToType(t) });
            //////}

            foreach (var item in source)
            {
                var elem = item.ToType(type);
                returnList.Add(elem);
            }



            //return List of T objects:
            return returnList;
        }

        public static object ToType(this object obj, Type type)
        {
            //create instance of T type object:
            //object tmp = Activator.CreateInstance(Type.GetType(type.ToString()));
            object tmp = Activator.CreateInstance(type);

            //loop through the properties of the object you want to covert:          
            foreach (PropertyInfo pi in obj.GetType().GetProperties())
            {
                try
                {
                    //get the value of property and try to assign it to the property of T type object:
                    tmp.GetType().GetProperty(pi.Name).SetValue(tmp, pi.GetValue(obj, null), null);
                }
                catch (Exception ex)
                {
                    //Logging.Log.Error(ex);
                }
            }
            //return the T type object:         
            return tmp;
        }


    }
}
