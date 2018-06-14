using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using System.Net;

namespace Core
{
    public interface IMsg
    {
        void ProcessMsg(byte[] buf);
    }

    [ProtoContract, Serializable]
    public enum DataAction
    {
        NONE = 0,
        DB_REG_MODEL = 1,
        DB_ADD = 2,
        DB_REMOVE = 3,
        DB_UPDATE = 4,
        DB_SELECT = 5,
        DB_RESULT = 6,
    }

    [ProtoContract, Serializable]
    public class Msg
    {
        [ProtoMember(1)]
        public long Id { get; set; }

        [ProtoMember(2)]
        public bool Status { get; set; }

        [ProtoMember(3)]
        public long SendID { get; set; }

        [ProtoMember(4)]
        public long ReceiveID { get; set; }

        [ProtoMember(5)]
        public long ReplyID { get; set; }

        [ProtoMember(6)]
        public string Title { get; set; }

        [ProtoMember(7)]
        public string Description { get; set; }

        [ProtoMember(8)]
        public string Message { get; set; }

        [ProtoMember(9)]
        public string DataField { get; set; }

        [ProtoMember(10)]
        public object Data { get; set; }

        [ProtoMember(11)]
        public DataAction DataAction = DataAction.NONE;

        [ProtoIgnore]
        public string DataType = string.Empty;
        [ProtoIgnore]
        public byte[] DataBuffer = null;
        [ProtoIgnore]
        public bool Deserialized = false;
        [ProtoIgnore]
        public Type _Type = null; 
    }

    [ProtoContract, Serializable]
    public class MsgSurrogate
    {
        //string is serializable so we'll just copy this property back and forth
        [ProtoMember(1)]
        public long Id { get; set; }

        [ProtoMember(2)]
        public bool Status { get; set; }

        [ProtoMember(3)]
        public long SendID { get; set; }

        [ProtoMember(4)]
        public long ReceiveID { get; set; }

        [ProtoMember(5)]
        public long ReplyID { get; set; }

        [ProtoMember(6)]
        public string Title { get; set; }

        [ProtoMember(7)]
        public string Description { get; set; }

        [ProtoMember(8)]
        public string Message { get; set; }

        [ProtoMember(9)]
        public string DataField { get; set; }

        [ProtoMember(10)]
        public DataAction DataAction = DataAction.NONE;

        //byte[] is serializable so we'll need to convert object to byte[] and back again
        [ProtoMember(11)]
        public byte[] DataBuffer { get; set; }

        [ProtoMember(12)]
        private string DataType = string.Empty;

        public static implicit operator Msg(MsgSurrogate suggorage)
        {
            if (suggorage == null) return null;

            string type = string.Empty;

            Msg m = new Msg
            {
                Id = suggorage.Id,
                Status = suggorage.Status,
                SendID = suggorage.SendID,
                ReceiveID = suggorage.ReceiveID,
                ReplyID = suggorage.ReplyID,
                Title = suggorage.Title,
                Description = suggorage.Description,
                Message = suggorage.Message,
                DataAction = suggorage.DataAction,
                DataType = suggorage.DataType,
                DataField = suggorage.DataField,
                Deserialized = false,
            };

            switch (m.DataAction)
            {
                case DataAction.DB_RESULT:
                    Type typeData = DbType.Get(suggorage.DataType);
                    if (typeData != null)
                    {
                        using (var stream = new MemoryStream(suggorage.DataBuffer))
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            m.Data = JsonSerializer.Create().Deserialize(reader, typeData);
                            m.Deserialized = true;
                        }
                    }
                    break;
                case DataAction.DB_SELECT:
                    m.Data = Encoding.UTF8.GetString(suggorage.DataBuffer);
                    m.Deserialized = true;
                    break;
                default:
                    var de = suggorage.DataBuffer.Deserialize_Object(suggorage.DataType);
                    if (de.OK)
                    {
                        m.Data = de.Data;
                        m.Deserialized = true;
                        if (m.DataType == "System.RuntimeType")
                            m.DataAction = DataAction.DB_REG_MODEL;
                    }
                    else
                        m.DataBuffer = suggorage.DataBuffer;
                    break;
            }

            return m;
        }

        public static implicit operator MsgSurrogate(Msg source)
        {
            if (source == null) return null;

            string type = "";
            if (source.Data != null) type = source.Data.GetType().FullName;

            MsgSurrogate m = new MsgSurrogate
            {
                Id = source.Id,
                Status = source.Status,
                SendID = source.SendID,
                ReceiveID = source.ReceiveID,
                ReplyID = source.ReplyID,
                Title = source.Title,
                Description = source.Description,
                Message = source.Message,
                DataAction = source.DataAction,
                DataField = source.DataField,
            };

            switch (m.DataAction)
            {
                case DataAction.DB_RESULT:
                    if (source._Type != null)
                    {
                        m.DataType = source._Type.FullName;
                        //m.DataField = string.Join(",", source._Type.GetProperties().Select(x => x.Name + "." + x.PropertyType.Name).ToArray());
                    }
                    else
                        m.DataType = source.DataType;
                    m.DataBuffer = Encoding.UTF8.GetBytes(source.Data as string);
                    break;
                case DataAction.DB_SELECT:
                    m.DataType = source.DataType;
                    m.DataBuffer = Encoding.UTF8.GetBytes(source.Data as string);
                    break;
                default:
                    m.DataType = type;
                    m.DataBuffer = source.Data.Serialize_Object();
                    break;
            }

            return m;
        }


    } //end class
}
