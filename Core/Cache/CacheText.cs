using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace Core.Cache
{
    public interface ICacheText
    { 
    
    }

    public class CacheText : ICacheText
    {
        private readonly ReaderWriterLockSlim lockRW;

        private readonly IDictionary<long, List<long>> storeIndex;
        private readonly IDictionary<long, string> storeData;
        private readonly List<string> wordAll;
        private readonly List<string> wordKey; 
        
        public CacheText()
        {
            wordAll = new List<string>();
            wordKey = new List<string>();
            lockRW = new ReaderWriterLockSlim();
            storeData = new Dictionary<long, string>();
            storeIndex = new Dictionary<long, List<long>>();
        }

        public int Count
        {
            get
            {
                return wordAll.Count;
            }
        }

        public string[] Search(string key) 
        {
            string[] tit = new string[] { };
            using (lockRW.ReadLock())
            {
                // do reading here
            }
            return tit;
        }

        public void Set(string html) 
        {
            using (lockRW.WriteLock())
            {
                string ascii = ToAscii(html).ToLower();
                string[] a = ascii.Split(' ').Distinct().ToArray();
                if (Count == 0)
                {
                    wordAll.AddRange(a);
                }
                else
                {
                    var a1 = a.Where(x => !wordAll.Any(o => o == x)).ToArray();
                    if (a1.Length > 0)
                        wordAll.AddRange(a1);
                }
            } 
        }
         
        private String ToAscii(string unicode)
        {
            if (string.IsNullOrEmpty(unicode)) return "";

            unicode = Regex.Replace(unicode, "[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            unicode = Regex.Replace(unicode, "[óòỏõọôồốổỗộơớờởỡợ]", "o");
            unicode = Regex.Replace(unicode, "[éèẻẽẹêếềểễệ]", "e");
            unicode = Regex.Replace(unicode, "[íìỉĩị]", "i");
            unicode = Regex.Replace(unicode, "[úùủũụưứừửữự]", "u");
            unicode = Regex.Replace(unicode, "[ýỳỷỹỵ]", "y");
            unicode = Regex.Replace(unicode, "[đ]", "d");
            unicode = Regex.Replace(unicode, "[-\\s+/]+", " ");
            //unicode = Regex.Replace(unicode, "\\W+", "_"); 
            return unicode;
        }
    }
}
