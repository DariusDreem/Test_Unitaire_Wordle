namespace Wordle.Domain;

/// <summary>
/// A valid Wordle word: exactly five letters A–Z, normalized to uppercase.
/// It is impossible to build an invalid <see cref="Word"/> — use <see cref="Create"/>,
/// which returns a failure instead of producing a malformed value.
/// As a value object, two words with the same letters are equal.
/// </summary>
public sealed record Word
{
    public const int Length = 5;

    public string Text { get; }

    private Word(string text) => Text = text;

    public static Result<Word, WordFormatError> Create(string raw)
    {
        if (raw is null || raw.Length != Length)
            return Result<Word, WordFormatError>.Failure(WordFormatError.WrongLength);

        var normalized = raw.ToUpperInvariant();
        if (!normalized.All(c => c is >= 'A' and <= 'Z'))
            return Result<Word, WordFormatError>.Failure(WordFormatError.NotAlphabetic);

        return Result<Word, WordFormatError>.Success(new Word(normalized));
    }

    /// <summary>The letter at the given position (0-based).</summary>
    public char this[int index] => Text[index];

    public override string ToString() => Text;
}
