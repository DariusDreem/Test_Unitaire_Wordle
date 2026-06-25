namespace Wordle.Domain;

/// <summary>Feedback for a single guessed letter at a given position.</summary>
public enum LetterFeedback
{
    /// <summary>Right letter, right position.</summary>
    Correct,

    /// <summary>Letter is in the secret, but at another position.</summary>
    Misplaced,

    /// <summary>Letter is not in the secret (or all its occurrences are already accounted for).</summary>
    Absent
}
