using System;
using System.Collections.Generic;

using System.Text;

namespace System.Binary
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class BinaryIncludeAttribute : Attribute
    {
        public BinaryIncludeAttribute(Type knownType, byte tag)
        {
            KnownType = knownType;
            Tag = tag;
        }
        public Type KnownType { get; private set; }
        public byte Tag { get; private set; }
    }
}
