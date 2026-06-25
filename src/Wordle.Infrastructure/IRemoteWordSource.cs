namespace Wordle.Infrastructure;

/// <summary>
/// A remote provider of candidate words (e.g. an HTTP API). Abstracted so the
/// loading logic — and especially the offline fallback — can be tested without
/// any real network access.
/// </summary>
public interface IRemoteWordSource
{
    Task<IReadOnlyList<string>> FetchWordsAsync(CancellationToken cancellationToken = default);
}
