namespace SagemExtrac
{
    public class SectionB_Entry
    {
        public byte[] Checksum { get; }
        public byte[] Data { get; }

        public int Size { get; private set; }

        public SectionB_Entry(BinaryReaderBE br)
        {
            br.ReadUInt32(); // EntrySizeSignature
            Size += 4;

            br.ReadUInt32(); // EntrySize
            Size += 4;

            br.ReadUInt32(); // MetadataSizeSignature
            Size += 4;

            var metaDataSize = br.ReadInt32();
            Size += 4;

            Checksum = br.ReadBytes(metaDataSize);
            Size += metaDataSize;

            br.ReadUInt32(); // DataSizeSignature
            Size += 4;

            var dataSize = br.ReadInt32();
            Size += 4;

            Data = br.ReadBytes(dataSize);
            Size += dataSize;
        }
    }
}