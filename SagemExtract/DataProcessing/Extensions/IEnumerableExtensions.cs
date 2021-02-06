using System.Collections.Generic;
using System.Linq;

namespace SagemExtract.DataProcessing.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<int> PatternAt(this IEnumerable<byte> source, byte[] pattern, int startAt = 0)
        {
            var enumerable = source as byte[] ?? source.ToArray();

            for (var i = startAt; i < enumerable.Length; i++)
            {
                if (enumerable.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return i;
                }
            }
        }
    }
}