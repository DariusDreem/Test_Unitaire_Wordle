namespace Wordle.Domain;

/// <summary>Lifecycle of a game. Only <see cref="InProgress"/> accepts new guesses.</summary>
public enum GameStatus
{
    InProgress,
    Won,
    Lost
}
