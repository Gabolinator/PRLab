using System.Text.RegularExpressions;
using PRLab.Domain.Utilities;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal static class WorkoutTextMatching
{
    public const string WordStart = @"(?<![\p{L}\p{M}\p{N}_])";
    public const string WordEnd = @"(?![\p{L}\p{M}\p{N}_])";

    public static Regex CreatePhrasePattern(string text, string start, string end)
    {
        var normalizedText = FormatingUtilities.NormalizeNonNullString(text);
        var words = normalizedText.Split(' ');
        var escapedWords = words.Select(Regex.Escape);
        var phrase = string.Join(@"[^\S\r\n]+", escapedWords);
        return new Regex($"{start}{phrase}{end}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    public static IEnumerable<TextSpan> Split(string text, string[] separators)
    {
        string[] parts = separators.Length == 0 ? [text] : text.Split(separators, StringSplitOptions.None);
        var offset = 0;
        foreach (var part in parts)
        {
            yield return new TextSpan(offset, part.Length);
            offset += part.Length;
            if (offset < text.Length)
            {
                var separator = separators.First(value =>
                    text.AsSpan(offset).StartsWith(value.AsSpan(), StringComparison.Ordinal));
                offset += separator.Length;
            }
        }
    }
}
