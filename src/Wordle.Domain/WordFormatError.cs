namespace Wordle.Domain;

/// <summary>
/// Why a raw string could not become a valid <see cref="Word"/>.
/// This is about the <em>format</em> of the word, not whether it exists in a dictionary.
/// </summary>
public enum WordFormatError
{
    /// <summary>The string is not exactly <see cref="Word.Length"/> characters long.</summary>
    WrongLength,

    /// <summary>The string contains characters that are not letters A–Z.</summary>
    NotAlphabetic
}
