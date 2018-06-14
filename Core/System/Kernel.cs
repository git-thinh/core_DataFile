using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Core
{
    public class Kernel
    {
        public struct CopyDataStruct : IDisposable
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;

            public void Dispose()
            {
                if (this.lpData != IntPtr.Zero)
                {
                    Kernel.LocalFree(this.lpData);
                    this.lpData = IntPtr.Zero;
                }
            }
        }

        public const int WM_COPYDATA = 0x004A;
        public static void Send(string msg, int receive_ID)
        {
            IntPtr targetHWnd = (IntPtr)receive_ID;
            CopyDataStruct cds = new CopyDataStruct();
            try
            {
                cds.cbData = (msg.Length + 1) * 2;
                cds.lpData = LocalAlloc(0x40, cds.cbData);
                Marshal.Copy(msg.ToCharArray(), 0, cds.lpData, msg.Length);
                cds.dwData = (IntPtr)1;
                SendMessage(targetHWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
            }
            finally
            {
                cds.Dispose();
            }
        }

        /// <summary>
        /// The SendMessage API
        /// </summary>
        /// <param name="hWnd">handle to the required window</param>
        /// <param name="Msg">the system/Custom message to send</param>
        /// <param name="wParam">first message parameter</param>
        /// <param name="lParam">second message parameter</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref CopyDataStruct lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalAlloc(int flag, int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr p);
    }
}
