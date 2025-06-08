using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VdfSharp.Data
{
    [DebuggerDisplay("{Key}, {Values}")]
    public class VdfProperty : VdfBase
    {
        public List<VdfBase> Values { get; set; }

        public VdfBase this[string key]
        {
            get => Values.First(i => i.Key == key);
            set
            {
                int index = Values.FindIndex(i => i.Key == key);
                if (index < 0)
                {
                    Values.Add(value);
                }
                else
                {
                    Values[index] = value;
                }
            }
        }

        public VdfBase TryGetValue(string key) => Values.FirstOrDefault(i => i.Key == key);
        public T TryGetValue<T>(string key) where T : VdfBase => Values.OfType<T>().FirstOrDefault(i => i.Key == key);
    }
}
