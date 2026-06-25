namespace Wordle.Domain.Tests;

/// <summary>
/// Hand-written test double for <see cref="IWordDictionary"/>. It fully controls
/// the secret word and the set of valid words, which makes every game test
/// deterministic (no randomness, no file/network access).
/// </summary>
internal sealed class FakeWordDictionary : IWordDictionary
{
    private readonly Word _secret;
    private readonly HashSet<string> _validWords;

    public FakeWordDictionary(Word secret, params Word[] otherValidWords)
    {
        _secret = secret;
        // The secret is, by definition, a valid word the player may guess.
        _validWords = otherValidWords.Append(secret).Select(word => word.Text).ToHashSet();
    }

    public bool Contains(Word word) => _validWords.Contains(word.Text);

    public Word PickSecret() => _secret;
}
