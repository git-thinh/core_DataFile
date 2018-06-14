using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Core.Http
{ 
    public interface IMsg
    {
        void ProcessMsg(byte[] buf);
    }

    [ProtoContract, Serializable]
    public enum MsgAction
    {
        NONE = 0,

        DB_ADD = 2,
        DB_REMOVE = 3,
        DB_UPDATE = 4,
        DB_SELECT = 5,
        DB_RESULT = 6,

        CMD_PING = 10,
        CMD_NOTIFICATION = 11,
        CMD_MESSAGEBOX = 12,
    }

    [ProtoContract, Serializable]
    public class Messager
    {
        [ProtoMember(1)]
        public long SendID { get; set; }

        [ProtoMember(2)]
        public long ReceiveID { get; set; }

        [ProtoMember(3)]
        public long ReplyID { get; set; }
    }

    [ProtoContract, Serializable]
    public class Msg
    {
        [ProtoMember(1)]
        public long Id { get; set; }

        [ProtoMember(2)]
        public bool Status { get; set; }

        [ProtoMember(3)]
        public Messager Messager { get; set; } 

        [ProtoMember(4)]
        public MsgAction Action { get; set; }

        [ProtoMember(5)]
        public MsgData Data { get; set; }
    } 

    [ProtoContract, Serializable]
    public class MsgData
    {  
        [ProtoMember(1)]
        public string Title { get; set; }

        [ProtoMember(2)]
        public string Description { get; set; }

        [ProtoMember(3)]
        public string Message { get; set; }

        [ProtoMember(4)]
        public string DataField { get; set; } // model.mUser;0Username,0Password

        [ProtoMember(5)]
        public object DataObject { get; set; } 
    }






}
