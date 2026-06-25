namespace Wordle.Domain;

/// <summary>Why a call to <see cref="Game.Play"/> was rejected (a business rule, not a crash).</summary>
public enum PlayError
{
    /// <summary>The guess is a well-formed word but is not in the dictionary.</summary>
    NotInDictionary,

    /// <summary>The game is already finished (won or lost); no further guess is accepted.</summary>
    GameOver
}
