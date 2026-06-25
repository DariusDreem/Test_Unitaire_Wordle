using Xunit;
using Wordle.Infrastructure;

namespace Wordle.Domain.Tests;

public class InMemoryWordDictionaryTests
{
    [Fact]
    public void It_knows_which_words_it_contains()
    {
        // Given a dictionary built from a small word list
        var dictionary = new InMemoryWordDictionary(new[] { AsWord("LIVRE"), AsWord("PORTE") });

        // Then it recognises words from the list and rejects the others
        Assert.True(dictionary.Contains(AsWord("LIVRE")));
        Assert.False(dictionary.Contains(AsWord("ZZZZZ")));
    }

    [Fact]
    public void Its_secret_is_always_a_word_it_contains()
    {
        // Given a dictionary with a single word (deterministic pick)
        var dictionary = new InMemoryWordDictionary(new[] { AsWord("LIVRE") }, new Random(1));

        // When a secret is picked
        var secret = dictionary.PickSecret();

        // Then it is a word the dictionary contains
        Assert.Equal(AsWord("LIVRE"), secret);
        Assert.True(dictionary.Contains(secret));
    }

    [Fact]
    public void It_cannot_be_built_from_an_empty_list()
    {
        // Then building a dictionary with no word is rejected
        Assert.Throws<ArgumentException>(() => new InMemoryWordDictionary(Array.Empty<Word>()));
    }

    private static Word AsWord(string raw) => Word.Create(raw).Value;
}
