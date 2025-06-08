using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VdfSharp.Data
{
    [DebuggerDisplay("String,{Key},{Value}")]
    public class BinaryVdfString : BinaryVdfBase
    {
        public string Value { get; set; }
    }
}
