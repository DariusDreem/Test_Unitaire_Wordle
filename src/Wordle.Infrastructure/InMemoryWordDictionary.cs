using Wordle.Domain;

namespace Wordle.Infrastructure;

/// <summary>
/// In-memory implementation of the <see cref="IWordDictionary"/> port. Once the
/// word list is loaded, validation and secret selection are pure in-memory
/// operations — no per-guess network calls.
/// </summary>
public sealed class InMemoryWordDictionary : IWordDictionary
{
    private readonly IReadOnlyList<Word> _words;
    private readonly HashSet<string> _index;
    private readonly Random _random;

    public InMemoryWordDictionary(IReadOnlyList<Word> words, Random? random = null)
    {
        if (words is null || words.Count == 0)
            throw new ArgumentException("The dictionary needs at least one word.", nameof(words));

        _words = words;
        _index = words.Select(word => word.Text).ToHashSet();
        _random = random ?? new Random();
    }

    public bool Contains(Word word) => _index.Contains(word.Text);

    public Word PickSecret() => _words[_random.Next(_words.Count)];
}
