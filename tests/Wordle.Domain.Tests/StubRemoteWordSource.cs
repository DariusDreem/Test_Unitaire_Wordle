using Wordle.Infrastructure;

namespace Wordle.Domain.Tests;

/// <summary>
/// Test double for <see cref="IRemoteWordSource"/>: either returns a fixed list,
/// or simulates an unreachable remote by throwing. No real HTTP involved.
/// </summary>
internal sealed class StubRemoteWordSource : IRemoteWordSource
{
    private readonly IReadOnlyList<string>? _words;

    private StubRemoteWordSource(IReadOnlyList<string>? words) => _words = words;

    public static StubRemoteWordSource Returning(params string[] words) => new(words);

    public static StubRemoteWordSource ThatFails() => new((IReadOnlyList<string>?)null);

    public Task<IReadOnlyList<string>> FetchWordsAsync(CancellationToken cancellationToken = default)
        => _words is null
            ? throw new HttpRequestException("simulated offline")
            : Task.FromResult(_words);
}
