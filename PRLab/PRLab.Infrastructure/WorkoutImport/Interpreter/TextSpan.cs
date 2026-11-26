namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal readonly record struct TextSpan(int Start, int Length)
{
    public int End => Start + Length;

    public string Read(string text) => text.Substring(Start, Length);

    public TextSpan OffsetBy(int offset) => new(Start + offset, Length);
}
