using System.Text;

namespace SagemExtrac
{
    public class SectionA_Entry
    {
        public string EntryName { get; }
        public byte[] Data { get; }

        public int Size { get; private set; }

        public SectionA_Entry(BinaryReaderBE reader)
        {
            reader.ReadUInt32(); // EntrySizeSignature
            Size += 4;

            reader.ReadUInt32(); // EntrySize
            Size += 4;

            reader.ReadUInt32(); // NameLengthSignature
            Size += 4;

            var nameLength = reader.ReadUInt32();
            Size += 4;

            var entryNameBytes = reader.ReadBytes((int)nameLength);
            Size += entryNameBytes.Length;

            EntryName = Encoding.UTF8.GetString(entryNameBytes);

            reader.ReadUInt32(); // DataLengthSignature
            Size += 4;

            var dataLength = reader.ReadInt32();
            Size += 4;

            Data = reader.ReadBytes(dataLength);
            Size += dataLength;
        }
    }
}