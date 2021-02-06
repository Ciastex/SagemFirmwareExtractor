using System.Text;
using SagemExtract.DataProcessing;

namespace SagemExtract.SagemFirmware
{
    public class FirmwareDataEntry
    {
        public byte[] Checksum { get; }

        public string FileName { get; }
        public byte[] Data { get; }

        public int Size { get; private set; }

        public FirmwareDataEntry(BinaryReaderBigEndian br)
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

            var data = br.ReadBytes(dataSize);
            Size += dataSize;

            var sb = new StringBuilder();
            foreach (var b in data)
            {
                sb.Append($"{b:X2}");
            }
            sb.Append(".bin");

            Data = data;
            FileName = sb.ToString();
        }
    }
}