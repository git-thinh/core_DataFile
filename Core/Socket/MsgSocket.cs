using System;
using System.Collections.Generic; 
using Fleck2.Interfaces;

namespace App
{
    public class MsgSocket
    {
        static readonly object _lock = new object();
        static Dictionary<long, IWebSocketConnection> dicClient = new Dictionary<long, IWebSocketConnection>() { };
        static Dictionary<string, long> dicName = new Dictionary<string, long>() { };

        private static void Join(MSG item, IWebSocketConnection client)
        {
            if (dicClient.ContainsKey(item.FromClientID) == false)
                lock (_lock)
                {
                    dicClient.Add(item.FromClientID, client);
                    if (dicName.ContainsKey(item.ToClientName) == false)
                        dicName.Add(item.ToClientName, item.FromClientID);
                }
        }
         
        private static void ReceiverData(MSG item)
        {
            switch (item.Type)
            { 
                case MSG_TYPE.CLIENT_CLOSE:
                    break;
                case MSG_TYPE.MSG_DATA:
                    if (item.ToClientName == "*")
                        BroadCast(item);
                    break;
            }
        }

        private static void BroadCast(MSG msg)
        {
            lock (_lock)
                foreach (var kv in dicClient)
                    kv.Value.Send(msg.Serialize());
        }
        
        public static void OnOpen(IWebSocketConnection client)
        {
            
        }

        public static void OnClose(IWebSocketConnection client)
        { 
        }

        public static void OnError(IWebSocketConnection client, Exception ex)
        { 
        }

        public static void OnMessage(IWebSocketConnection client, string data)
        {
            //client.Send("==>" + data);
            app.show_Notification(data);
            WndProc.Push(data);
        }


        public static void OnBinary(IWebSocketConnection client, byte[] buf)
        { 
            if (buf == null || buf.Length == 0) return;
            MSG item = MSG.Deserialize(buf);
            if (item == null) return;
            MSG_TYPE type = item.Type;
            if (type == MSG_TYPE.SET_ID_IN_APP
                || type == MSG_TYPE.SET_ID_IN_COMPUTER
                || type == MSG_TYPE.SET_ID_IN_LAN_INTERNET)
            {
                Join(item, client);
            }
            else
            {
                ReceiverData(item);
            }
        }
    }

}
