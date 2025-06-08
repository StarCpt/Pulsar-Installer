using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VdfSharp.Data
{
    [DebuggerDisplay("Map,{Key},{Values.Count}")]
    public class BinaryVdfMap : BinaryVdfBase
    {
        public List<BinaryVdfBase> Values { get; set; }
    }
}
