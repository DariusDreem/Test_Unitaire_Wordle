using System.Collections.Concurrent;
using Wordle.Domain;

namespace Wordle.Web;

/// <summary>
/// Keeps the in-progress games server-side (HTTP is stateless, the aggregate is mutable).
/// A real app would use a session/cache with expiry; in-memory is enough here.
/// </summary>
public sealed class GameStore
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public Guid Add(Game game)
    {
        var id = Guid.NewGuid();
        _games[id] = game;
        return id;
    }

    public Game? Find(Guid id) => _games.TryGetValue(id, out var game) ? game : null;
}
