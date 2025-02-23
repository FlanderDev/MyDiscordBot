
using System.Drawing;

namespace DiscordBot.Business.Helpers;

internal static class MessageHelper
{
    internal static Color[] GetColors(string text)
    {
        var clean = text.Length > 144 * 3 ? text[..(144 * 3)] : text;
        var charChunks = SplitIntoChunks(clean, 3);

        var results = charChunks
            .Where(w => w.Length == 3)
            .Select(s => Color.FromArgb(s[0], s[1], s[2]))
            .ToArray();

        return results;

        static T[][] SplitIntoChunks<T>(IEnumerable<T> source, int chunkSize)
        {
            if (chunkSize <= 0)
                throw new ArgumentException("Chunk size must be greater than 0.", nameof(chunkSize));

            return source.Select((value, index) => new { value, index })
                         .GroupBy(x => x.index / chunkSize)
                         .Select(g => g
                            .Select(x => x.value)
                            .ToArray())
                         .ToArray();
        }
    }
}
