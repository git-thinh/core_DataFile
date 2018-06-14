using System;
using System.Windows.Forms;
using System.Drawing; 

namespace Core
{
    public class FormLogger : Form //: BaseForm
    {
        private readonly string m_HR = Environment.NewLine + Environment.NewLine;
        private TextBox txt;

        public FormLogger() 
        {
            this.BackColor = Color.Black;

            Button btn = new Button() {
                Text = "Clear",
                Height = 30,
                Width = 80,
                BackColor = Color.WhiteSmoke,
            };
            btn.Click += (se, ev) => {
                //LogClear();
                //this.InvokeOnUiThreadIfRequired(() => { txt.Text = ""; });
            };

            this.Text = "LOG Viewer";
            this.Width = 999;
            this.Height = 400;
            this.Padding = new Padding(20, 40, 0, 0);

            txt = new TextBox()
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Consolas", 13.0F, FontStyle.Regular),
            };


            this.Controls.Add(btn);
            this.Controls.Add(txt);
            txt.BringToFront();

            //this.OnLogChange += FormLogger_OnLogChange;
        }

        private void FormLogger_OnLogChange(string msg, LogType type)
        {
            //AppendTextBox(msg);
        }

        public void AppendTextBox(byte[] buf, string subfix = "")
        {
            if (buf == null || buf.Length == 0) return;
             
        }

        public void ShowLog(LogSystem system, LogType type, string text)
        {
            if (txt.InvokeRequired == true)
                txt.Invoke((MethodInvoker)delegate
                {
                    txt.Text = text + m_HR + txt.Text;
                });
            else
                txt.Text = text + m_HR + txt.Text;
        }
    }
}
