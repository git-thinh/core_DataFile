using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Core
{
    public enum LogSystem
    {
        NONE = 0,
        HOST_SYSTEM = 1,
        HOST_WEB = 2,
        HOST_BOT = 3,
    }

    public enum LogType
    {
        NONE = 0,
        USER = 1,
        DATA_RESPONSE = 2,
        DATA_REQUEST = 3,
        PROCESS = 4,
        ERROR = 5,
        MESSAGEBOX = 6,
        CANCEL = 7,
        SUCCESS = 8,
        FINISH = 9,
    }
    
    public interface ILog 
    {
        void Write(LogSystem system, LogType type, string log);
    }

    public class Log: ILog
    {
        public event OnChangeEvent OnChange = null;
        public delegate void OnChangeEvent(LogSystem system, LogType type, string log);

        public void Write(LogSystem system, LogType type, string log)
        {
            if (OnChange != null)
                new Thread(() => OnChange(system, type, log)).Start();
        }
    }
}
