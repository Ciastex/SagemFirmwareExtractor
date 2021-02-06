using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SagemExtract.DataProcessing.Extensions;

namespace SagemExtract.SagemFirmware
{
    public class FileSystemScanner
    {
        private readonly byte[] _mfmSignature = new byte[]
        {
            0x6D, 0x66, 0x6D, 0x20,
            0x32, 0x2E, 0x30
        };

        private readonly byte[] _mfmHeaderEnd = new byte[] {0x0A, 0x2F};
        private readonly byte[] _contiguousRomData;

        public FileSystemScanner(byte[] contiguousRomData)
        {
            _contiguousRomData = contiguousRomData;
        }

        public void ExtractFileSystem(string targetDirectory)
        {
            var md5 = MD5.Create();
            
            using (var ms = new MemoryStream(_contiguousRomData))
            {
                using (var br = new BinaryReader(ms))
                {
                    foreach (var startIndex in _contiguousRomData.PatternAt(_mfmSignature))
                    {
                        var headerEnd = _contiguousRomData.PatternAt(
                            _mfmHeaderEnd, startIndex
                        ).ElementAt(0) + _mfmHeaderEnd.Length;

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
                            Path.Combine(targetDirectory, fileName),
                            fileData
                        );
                    }
                }
            }
        }
    }
}