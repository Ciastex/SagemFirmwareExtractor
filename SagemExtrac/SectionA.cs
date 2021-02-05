using System.Collections.Generic;

namespace SagemExtrac
{
    public class SectionA
    {
        public List<SectionA_Entry> Entries { get; } = new();

        public SectionA(BinaryReaderBE br)
        {
            var remainingLength = br.ReadInt32();

            while (remainingLength > 0)
            {
                var entry = new SectionA_Entry(br);
                Entries.Add(entry);

                remainingLength -= entry.Size;
            }
        }
    }
}