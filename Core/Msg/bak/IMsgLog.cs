using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public interface IMsgLog
    {
        void LogWrite(string text, MsgType type, string subfix);
        string LogGet(MsgType type, string hr);
        void LogClear(); 
    }
}
