namespace Wordle.Domain;

/// <summary>
/// The outcome for one letter of a guess: the letter that was played and how it scored.
/// A value object — equal when both the letter and the feedback are equal.
/// </summary>
public sealed record LetterEvaluation(char Letter, LetterFeedback Feedback);
