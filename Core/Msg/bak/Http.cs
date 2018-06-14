using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
 
using System.Linq;
using System.Linq.Dynamic;
using Newtonsoft.Json;

namespace Core
{
    public static class Http
    {
        public static byte[] Post(this string uri, NameValueCollection pairs)
        {
            byte[] response = null;
            try
            {
                using (WebClient client = new WebClient())
                {
                    response = client.UploadValues(uri, pairs);
                }
            }
            catch { }
            return response;
        }

        public static List<T> PostRAW<T>(this string uri, string raw_data)
        {
            byte[] response = null;

            WebRequest request = WebRequest.Create(uri);
            request.Method = "POST";

            byte[] byteArray = Encoding.UTF8.GetBytes(raw_data);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;

            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = 0;

            Stream dataStreamPost = request.GetRequestStream();
            dataStreamPost.Write(byteArray, 0, byteArray.Length);
            dataStreamPost.Close();

            WebResponse res = request.GetResponse();

            if (((HttpWebResponse)res).StatusCode == HttpStatusCode.OK)
            {
                //string s_status = ((HttpWebResponse)response).StatusDescription;

                Stream dataStreamResponse = res.GetResponseStream();
                StreamReader reader = new StreamReader(dataStreamResponse);
                string s = reader.ReadToEnd();
                reader.Close();
                dataStreamResponse.Close();

                if (!string.IsNullOrEmpty(s))
                {
                    try
                    {
                        //var a = JsonConvert.DeserializeObject<List<T>>(s);
                        //return a;
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            res.Close();

            return new List<T>() { };
        }

        public static string Post(this string uri)
        {
            byte[] response = null;

            WebRequest request = WebRequest.Create(uri);
            request.Method = "POST";

            //string postData = "This is a test that posts this string to a Web server.";
            //byte[] byteArray = Encoding.UTF8.GetBytes(postData); 
            //request.ContentType = "application/x-www-form-urlencoded"; 
            //request.ContentLength = byteArray.Length;

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = 0;

            //Stream dataStreamPost = request.GetRequestStream();
            //dataStreamPost.Write(byteArray, 0, byteArray.Length); 
            //dataStreamPost.Close();

            WebResponse res = request.GetResponse();

            if (((HttpWebResponse)res).StatusCode == HttpStatusCode.OK)
            {
                //string s_status = ((HttpWebResponse)response).StatusDescription;

                Stream dataStreamResponse = res.GetResponseStream();
                StreamReader reader = new StreamReader(dataStreamResponse);
                string s = reader.ReadToEnd();
                reader.Close();
                dataStreamResponse.Close();

                return s;
            }

            res.Close();

            return "";
        }


        public static List<T> Post<T>(this string uri)
        {
            try
            {
                byte[] response = null;

                WebRequest request = WebRequest.Create(uri);
                request.Method = "POST";

                //string postData = "This is a test that posts this string to a Web server.";
                //byte[] byteArray = Encoding.UTF8.GetBytes(postData); 
                //request.ContentType = "application/x-www-form-urlencoded"; 
                //request.ContentLength = byteArray.Length;

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = 0;

                //Stream dataStreamPost = request.GetRequestStream();
                //dataStreamPost.Write(byteArray, 0, byteArray.Length); 
                //dataStreamPost.Close();

                WebResponse res = request.GetResponse();

                if (((HttpWebResponse)res).StatusCode == HttpStatusCode.OK)
                {
                    //string s_status = ((HttpWebResponse)response).StatusDescription;

                    Stream dataStreamResponse = res.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStreamResponse);
                    string s = reader.ReadToEnd();
                    reader.Close();
                    dataStreamResponse.Close();

                    if (!string.IsNullOrEmpty(s))
                    {
                        try
                        {
                            //dynamic a0 = JsonArray.Parse(s);
                            //var li = a0.AsQueryable().Select("new(ma,tieu_de)").OrderBy("ma DESC").Cast<dynamic>().AsEnumerable().ToList();
                             
                            List<T> a = JsonConvert.DeserializeObject<List<T>>(s);
                            return a;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                res.Close();

            }
            catch (Exception ex)
            {

            }
            return new List<T>() { };
        }

    }//end class
}
