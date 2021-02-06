using System.Collections.Generic;
using System.IO;
using SagemExtract.DataProcessing;

namespace SagemExtract.SagemFirmware
{
    public class FirmwareDataSection
    {
        public List<FirmwareDataEntry> Entries { get; } = new();

        public FirmwareDataSection(BinaryReaderBigEndian br)
        {
            var remainingSize = br.ReadInt32();

            while (remainingSize > 0)
            {
                var entry = new FirmwareDataEntry(br);
                Entries.Add(entry);
                
                remainingSize -= entry.Size;
            }
        }
        
        public void SaveTo(string extractionDirectory)
        {
            var metadataDirectoryName = "data_section";
            var targetDirectoryPath = Path.Combine(
                extractionDirectory, 
                metadataDirectoryName
            );
            
            if (!Directory.Exists(targetDirectoryPath))
                Directory.CreateDirectory(targetDirectoryPath);

            foreach (var entry in Entries)
            {
                File.WriteAllBytes(
                    Path.Combine(targetDirectoryPath),
                    entry.Data
                );
            }            
        }
    }
}