using System;
using System.Collections.Generic;
using Fleck2.Interfaces;
using System.Linq;

namespace Core
{
    public interface IMsgConnect 
    { 
        void OnOpen(IWebSocketConnection client);
        void OnClose(IWebSocketConnection client);
        void OnError(IWebSocketConnection client, Exception ex);
        void OnMessage(IWebSocketConnection client, string data); 
        void OnBinary(IWebSocketConnection client, byte[] buf);    
    }

    public class MsgConnect: IMsgConnect
    {
        private readonly object _lock;
        private readonly Dictionary<long, IWebSocketConnection> conSocket;
        private readonly List<Tuple<long, string>> conName;

        private readonly ILog log;
        private readonly IMsgStore msg;

        public MsgConnect(ILog _log, IMsgStore _msg)
        {
            log = _log;
            msg = _msg;

            _lock = new object();
            conSocket = new Dictionary<long, IWebSocketConnection>() { };
            conName = new List<Tuple<long, string>>();
        }

        private void Join(Msg item, IWebSocketConnection client)
        {
            //if (dicClient.ContainsKey(item.FromClientID) == false)
            //    lock (_lock)
            //    {
            //        dicClient.Add(item.FromClientID, client);
            //        if (dicName.ContainsKey(item.ToClientName) == false)
            //            dicName.Add(item.ToClientName, item.FromClientID);
            //    }
        }

        private void ReceiverData(Msg item)
        {
            //switch (item.Type)
            //{ 
            //    case MSG_TYPE.CLIENT_CLOSE:
            //        break;
            //    case MSG_TYPE.MSG_DATA:
            //        if (item.ToClientName == "*")
            //            BroadCast(item);
            //        break;
            //}
        }

        private void BroadCast(Msg msg)
        {
            //lock (_lock)
            //    foreach (var kv in dicClient)
            //        kv.Value.Send(msg.Serialize());
        }





        public void Send(string name) 
        {
        
        }

        public void SetName(long id, string name) 
        {
            if (id <= 0 || string.IsNullOrEmpty(name)) return;
            name = name.ToLower().Trim();

            lock (_lock)
            {
                if (conSocket.ContainsKey(id))
                {
                    lock (conName)
                    {
                        int index = conName.FindIndex(x => x.Item1 == id);
                        if (index == -1)
                            conName.Add(new Tuple<long, string>(id, name));
                        else
                            conName[index] = new Tuple<long, string>(id, name);
                    }
                }
            }
        }

        public void OnOpen(IWebSocketConnection client)
        {
            client.Send(BitConverter.GetBytes(client.ConnectionInfo.Id));
            lock (_lock) if (!conSocket.ContainsKey(client.ConnectionInfo.Id)) conSocket.Add(client.ConnectionInfo.Id, client);
            log.Write(LogSystem.HOST_SYSTEM, LogType.USER, string.Format("Opened websocket client: {0}", client.ConnectionInfo.Id));
        }

        public void OnClose(IWebSocketConnection client)
        {
            lock (_lock) if (conSocket.ContainsKey(client.ConnectionInfo.Id)) conSocket.Remove(client.ConnectionInfo.Id);
            log.Write(LogSystem.HOST_SYSTEM, LogType.USER, string.Format("Closed websocket client: {0}", client.ConnectionInfo.Id));
        }

        public void OnError(IWebSocketConnection client, Exception ex)
        {
            log.Write(LogSystem.HOST_SYSTEM, LogType.USER, string.Format("Exception websocket client: {0} \n {1}", client.ConnectionInfo.Id, ex.Message));
        }

        public void OnMessage(IWebSocketConnection client, string data)
        {
            log.Write(LogSystem.HOST_SYSTEM, LogType.USER, string.Format("Client {0} send text: {1}", client.ConnectionInfo.Id, data));

            //////client.Send("==>" + data);
            ////app.show_Notification(data);
            ////WndProc.Push(data);
        }


        public void OnBinary(IWebSocketConnection client, byte[] buf)
        { 
            log.Write(LogSystem.HOST_SYSTEM, LogType.USER, string.Format("Client {0} send binary: {1}",
                client.ConnectionInfo.Id, 
                string.Join(" ", buf.Select(x => x.ToString()).ToArray())));

            ////if (buf == null || buf.Length == 0) return;
            ////MSG item = MSG.Deserialize(buf);
            ////if (item == null) return;
            ////MSG_TYPE type = item.Type;
            ////if (type == MSG_TYPE.SET_ID_IN_APP
            ////    || type == MSG_TYPE.SET_ID_IN_COMPUTER
            ////    || type == MSG_TYPE.SET_ID_IN_LAN_INTERNET)
            ////{
            ////    Join(item, client);
            ////}
            ////else
            ////{
            ////    ReceiverData(item);
            ////}
        }
    }

}
