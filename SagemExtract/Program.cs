using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SagemExtract.SagemFirmware;

namespace SagemExtract
{
    class Program
    {
        // 0x00001000: SectionA_Length
        // 0x00001100: SectionA_EntryLength (minus signature and length)
        // 0x00001110: SectionA_Entry_NameLength
        // 0x00001120: SectionA_Entry_DataLength
        // 0x00002000: SectionB_Length
        // 0x00002100: SectionB_EntryLength
        // 0x00002110: SectionB_Entry_ChecksumLength
        // 0x00002111: SectionB_Entry_DataLength

        private static bool _saveChunks;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Provide a file.");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File doesn't exist.");
                return;
            }

            if (args.Contains("--save-chunks"))
                _saveChunks = true;

            var targetDir = Path.Combine(AppContext.BaseDirectory, $"{args[0]}.extracted");
            var targetFsDir = Path.Combine(targetDir, "file_system");

            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            if (!Directory.Exists(targetFsDir))
                Directory.CreateDirectory(targetFsDir);

            var firmwareFile = new FirmwareFile(args[0]);
            
            var fileSystemScanner = new FileSystemScanner(firmwareFile.ContiguousRomData);
            fileSystemScanner.ExtractFileSystem(targetFsDir);
        }
    }
}