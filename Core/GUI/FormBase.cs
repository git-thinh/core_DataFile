using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Core
{
    public class FormBase : Form, IMsgLog
    {
        public event OnDataReceiveEvent OnDataReceive = null;
        public delegate void OnDataReceiveEvent(string data);

        private IMsgLog m_store = null;
         
        public IMsgLog Store
        {
            get { return m_store; }
        }

        public FormBase( IMsgLog store)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            m_store = store;
        }

        public void SetIcon(Icon icon)
        {
            if (icon != null)
                this.Icon = icon;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Kernel.WM_COPYDATA:
                    Kernel.CopyDataStruct st = (Kernel.CopyDataStruct)Marshal.PtrToStructure(m.LParam, typeof(Kernel.CopyDataStruct));
                    string data = Marshal.PtrToStringUni(st.lpData);
                    if (!string.IsNullOrEmpty(data) && OnDataReceive != null)
                        OnDataReceive(data);
                    break;
                default:
                    // let the base class deal with it
                    base.WndProc(ref m);
                    break;
            }
        }

        

        #region [ === Interface IMsg === ]

        public void LogWrite(string text, MsgType type)
        {
            m_store.LogWrite(text, type, string.Empty);
        }

        public void LogWrite(string text, MsgType type, string subfix)
        {
            m_store.LogWrite(text, type, subfix);
        }

        public string LogGet(MsgType type, string hr)
        {
            return m_store.LogGet(type, hr);
        }

        public void LogClear()
        {
            m_store.LogClear();
        }
        
        #endregion
    }
}
