using System;
using System.Collections.Generic;
using System.IO;

using System.Text;

namespace System.Binary
{
    public interface ISerializer<T> {
        byte[] Serialize(T obj);
        T Deserialize(byte[] buffer);
        void Serialize(T obj, Stream stream);
        T Deserialize(Stream stream);
    }
}
