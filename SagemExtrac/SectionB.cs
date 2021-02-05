using System.Collections.Generic;

namespace SagemExtrac
{
    public class SectionB
    {
        public List<SectionB_Entry> Entries { get; } = new();

        public SectionB(BinaryReaderBE br)
        {
            var remainingSize = br.ReadInt32();

            while (remainingSize > 0)
            {
                var entry = new SectionB_Entry(br);
                Entries.Add(entry);
                
                remainingSize -= entry.Size;
            }
        }
    }
}