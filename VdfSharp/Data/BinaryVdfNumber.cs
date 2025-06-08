using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VdfSharp.Data
{
    [DebuggerDisplay("Number,{Key},{Value}")]
    public class BinaryVdfNumber : BinaryVdfBase
    {
        public uint Value { get; set; }
    }
}
