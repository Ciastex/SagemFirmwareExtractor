using System.Collections.Generic;
using System.IO;
using SagemExtract.DataProcessing;

namespace SagemExtract.SagemFirmware
{
    public class FirmwareMetadataSection
    {
        public List<FirmwareMetadataEntry> Entries { get; } = new();

        public FirmwareMetadataSection(BinaryReaderBigEndian br)
        {
            var remainingLength = br.ReadInt32();

            while (remainingLength > 0)
            {
                var entry = new FirmwareMetadataEntry(br);
                Entries.Add(entry);

                remainingLength -= entry.Size;
            }
        }

        public void SaveTo(string extractionDirectory)
        {
            var metadataDirectoryName = "metadata_section";
            var targetDirectoryPath = Path.Combine(
                extractionDirectory, 
                metadataDirectoryName
            );
            
            if (!Directory.Exists(targetDirectoryPath))
                Directory.CreateDirectory(targetDirectoryPath);

            foreach (var entry in Entries)
            {
                File.WriteAllBytes(
                    Path.Combine(targetDirectoryPath, entry.FileName),
                    entry.Data
                );
            }            
        }
    }
}