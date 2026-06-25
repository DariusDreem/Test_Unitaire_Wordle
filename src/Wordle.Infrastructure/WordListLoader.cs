using Wordle.Domain;

namespace Wordle.Infrastructure;

/// <summary>
/// Builds the list of playable <see cref="Word"/>s once, at startup:
/// it tries the remote source first and, on any failure or empty result,
/// falls back to a local list. Every candidate must pass <see cref="Word.Create"/>,
/// so only well-formed five-letter words enter the dictionary.
/// </summary>
public static class WordListLoader
{
    public static async Task<IReadOnlyList<Word>> LoadAsync(
        IRemoteWordSource remoteSource,
        IEnumerable<string> fallback,
        CancellationToken cancellationToken = default)
    {
        var fromRemote = await TryFetchAsync(remoteSource, cancellationToken);
        var words = ToValidWords(fromRemote);

        if (words.Count == 0)
            words = ToValidWords(fallback);

        return words;
    }

    private static async Task<IReadOnlyList<string>> TryFetchAsync(
        IRemoteWordSource source, CancellationToken cancellationToken)
    {
        try
        {
            return await source.FetchWordsAsync(cancellationToken);
        }
        catch
        {
            // Network down, timeout, bad response… the local list will take over.
            return Array.Empty<string>();
        }
    }

    private static List<Word> ToValidWords(IEnumerable<string> raw) =>
        raw.Select(Word.Create)
           .Where(result => result.IsSuccess)
           .Select(result => result.Value)
           .Distinct()
           .ToList();
}
