namespace Wordle.Domain;

/// <summary>
/// A single game of Wordle: the aggregate root that enforces the rules.
/// Mutable — each accepted guess advances its state. The dictionary it relies on
/// is injected, so the game stays independent of how words are stored.
/// </summary>
public sealed class Game
{
    public const int MaxAttempts = 6;

    private readonly IWordDictionary _dictionary;
    private readonly List<GuessResult> _history = new();

    public Game(IWordDictionary dictionary)
    {
        _dictionary = dictionary;
        Secret = dictionary.PickSecret();
    }

    public Word Secret { get; }

    public GameStatus Status { get; private set; } = GameStatus.InProgress;

    public IReadOnlyList<GuessResult> History => _history;

    /// <summary>How many guesses the player still has. Derived from the history.</summary>
    public int RemainingAttempts => MaxAttempts - _history.Count;

    /// <summary>
    /// Plays a guess. Returns the feedback on success, or a <see cref="PlayError"/>
    /// when the guess is refused. A refused guess never consumes an attempt.
    /// </summary>
    public Result<GuessResult, PlayError> Play(Word guess)
    {
        if (Status != GameStatus.InProgress)
            return Result<GuessResult, PlayError>.Failure(PlayError.GameOver);

        if (!_dictionary.Contains(guess))
            return Result<GuessResult, PlayError>.Failure(PlayError.NotInDictionary);

        var result = GuessResult.For(Secret, guess);
        _history.Add(result);

        if (result.IsWin)
            Status = GameStatus.Won;
        else if (_history.Count >= MaxAttempts)
            Status = GameStatus.Lost;

        return Result<GuessResult, PlayError>.Success(result);
    }
}
