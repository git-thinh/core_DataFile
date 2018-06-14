using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{ 
    public enum MsgType
    {
        USER = 1,
        DATA = 2,
        PROCESS = 3,
        ERROR = 4,
        SYSTEM = 5,
        MSG = 7,
        SUCCESS = 8,
        FINISH = 9,
    }
}
