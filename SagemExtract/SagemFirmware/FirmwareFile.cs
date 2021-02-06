using System;
using System.Collections.Generic;
using System.IO;
using SagemExtract.DataProcessing;

namespace SagemExtract.SagemFirmware
{
    public class FirmwareFile
    {
        public int Version { get; private set; }
        public int CumulativeSectionSize { get; private set; }

        public FirmwareMetadataSection MetadataSection { get; }
        public FirmwareDataSection DataSection { get; }

        public byte[] ContiguousRomData { get; }

        public FirmwareFile(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var br = new BinaryReaderBigEndian(fs))
                {
                    Version = br.ReadInt32();
                    CumulativeSectionSize = br.ReadInt32();

                    var sig = br.ReadUInt32();
                    if (sig == 0x00001000)
                    {
                        MetadataSection = new FirmwareMetadataSection(br);
                    }
                    else
                    {
                        throw new FormatException($"Expected SectionA signature 0x00001000, got 0x{sig:X8}");
                    }

                    sig = br.ReadUInt32();
                    if (sig == 0x00002000)
                    {
                        DataSection = new FirmwareDataSection(br);
                    }
                    else
                    {
                        throw new FormatException($"Expected SectionB signature 0x00002000, got 0x{sig:X8}");
                    }
                }
            }

            var romBytes = new List<byte>();

            foreach (var entry in DataSection.Entries)
            {
                if (entry.Data.Length > 65000)
                    romBytes.AddRange(entry.Data);
            }

            ContiguousRomData = romBytes.ToArray();
        }

        public void SaveContiguousROM(string filePath)
        {
            File.WriteAllBytes(
                filePath,
                ContiguousRomData
            );
        }
    }
}