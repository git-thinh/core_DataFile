using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{ 
    public sealed class MsgProxy : MarshalByRefObject, IMsg
    {
        public bool Ping()
        {
            throw new NotImplementedException();
        }

        public bool Connect(string url)
        {
            throw new NotImplementedException();
        }

        public string Send(Msg m)
        {
            throw new NotImplementedException();
        }
    }
}
