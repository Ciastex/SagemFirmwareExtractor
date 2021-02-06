using System.Collections.Generic;

namespace SagemExtract.DataProcessing
{
    public static class FileTypeRecognizer
    {
        private class Signature
        {
            public byte[] BinarySig { get; }
            public string Extension { get; }
            public int OffsetInFile { get; }

            public Signature(byte[] sig, string ext, int offset)
            {
                BinarySig = sig;
                Extension = ext;
                OffsetInFile = offset;
            }
        }

        private static List<Signature> _signatures = new()
        {
            new Signature(new byte[] {0x4d, 0x54, 0x68, 0x64}, "mid", 0),
            new Signature(new byte[] {0x52, 0x49, 0x46, 0x46}, "wav", 0),
            new Signature(new byte[] {0xFF, 0xD8, 0xFF, 0xE0}, "jpg", 0),
            new Signature(new byte[] {0xFF, 0xD8, 0xFF, 0xE1}, "jpg", 0),
            new Signature(new byte[] {0xFF, 0xF9}, "aac", 0),
            new Signature(new byte[] {0xFF, 0xF1}, "aac", 0),
            new Signature(new byte[] {0x46, 0x57, 0x53}, "swf", 0),
            new Signature(new byte[] {0x66, 0x74, 0x79, 0x70, 0x33, 0x67, 0x70}, "3gp", 4),
            new Signature(new byte[] {0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32}, "mp4", 4),
            new Signature(new byte[] {0x47, 0x49, 0x46, 0x38, 0x39, 0x61}, "gif", 0),
            new Signature(new byte[] {0x23, 0x21, 0x41, 0x4D, 0x52}, "amr", 0),
        };

        static FileTypeRecognizer()
        {
            for (var i = 0xF0; i < 0xFF; i++)
            {
                _signatures.Add(new Signature(new byte[] {0xFF, (byte)i}, "mp3", 0));
                _signatures.Add(new Signature(new byte[] {0xFF, (byte)(i - 0x10)}, "mp3", 0));
            }
        }

        public static string TryRecognizeFileExtension(byte[] data)
        {
            foreach (var sig in _signatures)
            {
                if (IsMagicMatch(sig.BinarySig, data, sig.OffsetInFile))
                    return sig.Extension;
            }

            return "unk";
        }

        private static bool IsMagicMatch(byte[] sig, byte[] data, int offset)
        {
            if (data.Length < sig.Length)
                return false;

            for (var i = 0; i < sig.Length; i++)
            {
                if (sig[i] != data[i + offset])
                    return false;
            }

            return true;
        }
    }
}