using System.Text;
using SagemExtract.DataProcessing;

namespace SagemExtract.SagemFirmware
{
    public class FirmwareMetadataEntry
    {
        public string FileName { get; }
        public byte[] Data { get; }

        public int Size { get; private set; }

        public FirmwareMetadataEntry(BinaryReaderBigEndian reader)
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

            FileName = Encoding.UTF8.GetString(entryNameBytes);

            reader.ReadUInt32(); // DataLengthSignature
            Size += 4;

            var dataLength = reader.ReadInt32();
            Size += 4;

            Data = reader.ReadBytes(dataLength);
            Size += dataLength;
        }
    }
}