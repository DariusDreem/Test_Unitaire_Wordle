using Xunit;
using Wordle.Infrastructure;

namespace Wordle.Domain.Tests;

public class WordListLoaderTests
{
    [Fact]
    public async Task It_keeps_only_well_formed_five_letter_words_from_the_remote_source()
    {
        // Given a remote source returning a mix of good and bad entries
        var remote = StubRemoteWordSource.Returning("LIVRE", "PORTE", "TROP", "ECOLE", "ÉCOLE", "HELLOO");

        // When the word list is loaded
        var words = await WordListLoader.LoadAsync(remote, fallback: new[] { "TABLE" });

        // Then only the valid five-letter A–Z words are kept, in order
        Assert.Equal(new[] { "LIVRE", "PORTE", "ECOLE" }, words.Select(word => word.Text));
    }

    [Fact]
    public async Task It_falls_back_to_the_local_list_when_the_remote_source_is_unavailable()
    {
        // Given a remote source that is offline (it throws)
        var remote = StubRemoteWordSource.ThatFails();

        // When the word list is loaded
        var words = await WordListLoader.LoadAsync(remote, fallback: new[] { "LIVRE", "PORTE" });

        // Then the local fallback list is used instead
        Assert.Equal(new[] { "LIVRE", "PORTE" }, words.Select(word => word.Text));
    }

    [Fact]
    public async Task It_falls_back_when_the_remote_source_has_no_usable_words()
    {
        // Given a remote source returning only invalid entries
        var remote = StubRemoteWordSource.Returning("TROP", "AB", " ", "ÉLÈVE");

        // When the word list is loaded
        var words = await WordListLoader.LoadAsync(remote, fallback: new[] { "TABLE" });

        // Then the fallback is used
        Assert.Equal(new[] { "TABLE" }, words.Select(word => word.Text));
    }
}
