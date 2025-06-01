using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VdfSharp.Data
{
    [DebuggerDisplay("{Key}, {Value}")]
    public class VdfKeyValue : VdfBase
    {
        public string Value { get; set; }
    }
}
