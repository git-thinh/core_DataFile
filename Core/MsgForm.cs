using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Core
{

    public sealed class MsgFormProxy : MarshalByRefObject, IMsgForm
    {
        public string Push(MsgFormRequest request)
        {
            throw new NotImplementedException();
        }

        public bool Ping()
        {
            throw new NotImplementedException();
        }
    }
     

    [ProtoContract]
    public sealed class MsgFormRequest
    {
        [ProtoMember(1)]
        public bool Status { get; set; }

        [ProtoMember(2)]
        public int FormID { get; set; }

        [ProtoMember(3)]
        public string Message { get; set; }

        //[ProtoMember(4, DynamicType = true)]
        //public object Data { get; set; }
    }
     

    public interface IMsgForm
    {
        bool Ping();
        string Push(MsgFormRequest request);
    } 
}
