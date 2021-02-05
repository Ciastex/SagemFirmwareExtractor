using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SagemExtrac
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

            var fs = new FileStream(args[0], FileMode.Open);

            SectionA sectionA;
            SectionB sectionB;

            using (var br = new BinaryReaderBE(fs))
            {
                var fileVersion = br.ReadUInt32();
                Console.WriteLine($"File version {fileVersion}.");

                var binDirSize = br.ReadUInt32();
                Console.WriteLine($"Binary directory size {binDirSize}");

                var sig = br.ReadUInt32();
                if (sig == 0x00001000)
                {
                    sectionA = new SectionA(br);
                }
                else
                {
                    Console.WriteLine($"Expected SectionA signature 0x00001000, got 0x{sig:X8}");
                    return;
                }

                sig = br.ReadUInt32();
                if (sig == 0x00002000)
                {
                    sectionB = new SectionB(br);
                }
                else
                {
                    Console.WriteLine($"Expected SectionB signature 0x00002000, got 0x{sig:X8}");
                    return;
                }
            }

            Console.WriteLine($"Section A ({sectionA.Entries.Count} entries)\n---------");
            foreach (var file in sectionA.Entries)
            {
                Console.WriteLine(file.EntryName);

                if (_saveChunks)
                {
                    File.WriteAllBytes(
                        Path.Combine(targetDir, file.EntryName),
                        file.Data
                    );
                }
            }

            var sb = new StringBuilder();
            Console.WriteLine($"Section B ({sectionB.Entries.Count} entries)\n---------");
            var romBytes = new List<byte>();

            foreach (var file in sectionB.Entries)
            {
                sb.Clear();

                foreach (var b in file.Checksum)
                {
                    sb.Append($"{b:X2}");
                }

                var fileName = sb.ToString();

                Console.WriteLine(fileName);
                if (_saveChunks)
                {
                    File.WriteAllBytes(
                        Path.Combine(targetDir, $"{fileName}.bin"),
                        file.Data
                    );
                }

                romBytes.AddRange(file.Data);
            }

            var romArray = romBytes.ToArray();

            Console.WriteLine($"ROM file saved to {args[0]}.ROM");
            File.WriteAllBytes(
                Path.Combine(targetDir, args[0] + ".ROM"),
                romArray
            );

            var ms = new MemoryStream(romArray);
            ms.Seek(0, SeekOrigin.Begin);

            var multimediaFileManagerSignature = new byte[]
            {
                0x6d, 0x66, 0x6d, 0x20,
                0x32, 0x2e, 0x30
            };

            var mfmHeaderEnd = new byte[] {0x0A, 0x2F};

            var md5 = MD5.Create();

            using (var br = new BinaryReader(ms))
            {
                foreach (var startIndex in PatternAt(romArray, multimediaFileManagerSignature))
                {
                    var headerEnd = PatternAt(romArray, mfmHeaderEnd, startIndex).ElementAt(0) + mfmHeaderEnd.Length;
                    ms.Seek(startIndex, SeekOrigin.Begin);

                    var header = Encoding.ASCII.GetString(
                        br.ReadBytes(headerEnd - startIndex)
                    );

                    var headerEntries = header.Split('\n');
                    var dataLength = int.Parse(headerEntries[1].Split(' ')[0]);

                    var fileData = br.ReadBytes(dataLength);
                    var checksum = md5.ComputeHash(fileData);

                    var fileName = string.Empty;
                    foreach (var b in checksum)
                        fileName += $"{b:X2}";

                    File.WriteAllBytes(
                        Path.Combine(targetFsDir, fileName),
                        fileData
                    );
                }
            }
        }

        private static IEnumerable<int> PatternAt(byte[] source, byte[] pattern, int startAt = 0)
        {
            for (var i = startAt; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return i;
                }
            }
        }
    }
}