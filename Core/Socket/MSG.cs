using System;
using System.Linq;
using WebSocketSharp; 
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace App
{
    [Serializable()]
    public class MSG
    { 
        public MSG_TYPE Type { set; get; }
         
        public long FromClientID { set; get; }
         
        public bool Reply { set; get; }
         
        public string ToClientName { set; get; }
         
        public object Data { set; get; }

        public byte[] Serialize()
        {
            byte[] buf = new byte[] { };
            try
            {
                using (var stream = new MemoryStream())
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    using (var ms = new MemoryStream())
                    {
                        bf.Serialize(ms, this);
                        buf = ms.ToArray();
                    }
                }
            }
            catch { }
            return buf;
        }

        public static MSG Deserialize(byte[] message)
        {
            MSG it = null;
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (var ms = new MemoryStream(message))
                {
                    it = (MSG)bf.Deserialize(ms);
                }
            }
            catch { }
            return it;
        }
    }

    [Serializable()]
    public enum MSG_TYPE
    {
        NONE = 0,
        SET_ID_IN_APP = 1,
        SET_ID_IN_COMPUTER = 2,
        SET_ID_IN_LAN_INTERNET = 3,

        MSG_DATA = 4,
        PING = 5,

        TEXT_ASCII = 6,
        TEXT_UTF8 = 7,
        TEXT_UNICODE = 8,
        CLIENT_CLOSE = 9,
    }


    public static class msg_ext
    {
        public static long JoinClient(this WebSocket client, MSG_TYPE TYPE_ID, string client_Name)
        {
            long id = 0;
            byte type = (byte)TYPE_ID;
            long.TryParse(type.ToString() + DateTime.Now.ToString("0yyyyMMddHHmmssfff"), out id);

            MSG it = new MSG() { ToClientName = client_Name, FromClientID = id, Type = TYPE_ID };
            byte[] buf = it.Serialize();
            client.Send(buf);

            return id;
        }

        public static void SendToClientName(this WebSocket client, string toClientName, object data)
        {
            MSG it = new MSG() { Data = data, ToClientName = toClientName, FromClientID = client.ID, Type = MSG_TYPE.MSG_DATA };
            byte[] buf = it.Serialize();
            client.Send(buf);
        }

        public static void BroadCast(this WebSocket client, object data)
        {
            MSG it = new MSG() { Data = data, ToClientName = "*", FromClientID = client.ID, Type = MSG_TYPE.MSG_DATA };
            byte[] buf = it.Serialize();
            client.Send(buf);
        }
    }
}
