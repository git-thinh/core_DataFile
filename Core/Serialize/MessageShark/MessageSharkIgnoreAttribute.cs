using System;
using System.Collections.Generic;

using System.Text;

namespace System.Binary
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BinaryIgnoreAttribute : Attribute
    {
    }
}
