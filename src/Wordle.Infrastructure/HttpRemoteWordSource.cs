namespace Wordle.Infrastructure;

/// <summary>
/// Fetches a plain-text word list (one word per line) from a URL over HTTP.
/// Any failure is left to the caller (<see cref="WordListLoader"/>), which falls
/// back to the local list.
/// </summary>
public sealed class HttpRemoteWordSource : IRemoteWordSource
{
    private readonly HttpClient _http;
    private readonly Uri _url;

    public HttpRemoteWordSource(HttpClient http, Uri url)
    {
        _http = http;
        _url = url;
    }

    public async Task<IReadOnlyList<string>> FetchWordsAsync(CancellationToken cancellationToken = default)
    {
        var content = await _http.GetStringAsync(_url, cancellationToken);
        return content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
