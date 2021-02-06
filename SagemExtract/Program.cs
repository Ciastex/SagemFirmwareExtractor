using System;
using System.IO;
using System.Linq;
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

        private static bool _extractExe;
        private static bool _dontExtractFirmware;

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

            if (args.Contains("--extract-exe"))
            {
                _extractExe = true;
                
                if (args.Contains("--fw-noextract"))
                    _dontExtractFirmware = true;
            }

            var firmwarePath = args[0];
            if (_extractExe)
            {
                try
                {
                    ExtractFirmwareFromExecutable(firmwarePath, out firmwarePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to extract the firmware from EXE: {e.Message}.");
                    return;
                }
            }

            if (!_dontExtractFirmware)
            {
                try
                {
                    ExtractFirmware(firmwarePath);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to extract the firmware: {e.Message}");
                }
            }
        }

        static void ExtractFirmwareFromExecutable(string filePath, out string firmwarePath)
        {
            firmwarePath = string.Empty;

            var fs = new FileStream(filePath, FileMode.Open);
            var ms = new MemoryStream();
            fs.CopyTo(ms);
            fs.Seek(0, SeekOrigin.Begin);
            var peFile = new PEHandler.PEFile(fs, 1024);
            var entry = peFile.RsrcHandler.GetEntryFromPath("PBCS/1/0");

            if (entry == null)
            {
                throw new Exception("PBCS resource entry not found, cannot extract from installer executable.");
            }
            
            firmwarePath = Path.Combine(
                Environment.CurrentDirectory,
                Path.GetFileNameWithoutExtension(filePath) + ".ROM"
            );

            File.WriteAllBytes(firmwarePath, entry.Data);
        }

        static void ExtractFirmware(string filePath)
        {
            var targetDir = Path.Combine(Environment.CurrentDirectory, $"{filePath}.extracted");
            var targetFsDir = Path.Combine(targetDir, "file_system");

            if (!Directory.Exists(targetFsDir))
                Directory.CreateDirectory(targetFsDir);

            var firmwareFile = new FirmwareFile(filePath);

            var fileSystemScanner = new FileSystemScanner(firmwareFile.ContiguousRomData);
            fileSystemScanner.ExtractFileSystem(targetFsDir);
        }
    }
}