using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public interface ISampleService 
    { 
         int GetSquare(int x);
         string GetDate();    
    }

    public class SampleService : ISampleService
    {
        public int GetSquare(int x) { return x * x; }

        public string GetDate()
        {
            return DateTime.Now.ToLongDateString();
        }
    }

}
