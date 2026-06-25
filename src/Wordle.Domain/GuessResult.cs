namespace Wordle.Domain;

/// <summary>
/// The evaluation of a whole guess against the secret word: one
/// <see cref="LetterEvaluation"/> per position. Pure — it depends only on the
/// two words, never on any infrastructure.
/// </summary>
public sealed class GuessResult
{
    private readonly LetterEvaluation[] _letters;

    private GuessResult(LetterEvaluation[] letters) => _letters = letters;

    public IReadOnlyList<LetterEvaluation> Letters => _letters;

    /// <summary>True when every letter is <see cref="LetterFeedback.Correct"/>.</summary>
    public bool IsWin => _letters.All(l => l.Feedback == LetterFeedback.Correct);

    /// <summary>
    /// Evaluates <paramref name="guess"/> against <paramref name="secret"/> using the
    /// two-pass rule so that a letter is never scored more times than it occurs in the secret:
    /// <list type="number">
    /// <item>mark every exact match CORRECT and consume that occurrence from the secret;</item>
    /// <item>for the rest, mark MISPLACED while the letter still has occurrences left, otherwise ABSENT.</item>
    /// </list>
    /// </summary>
    public static GuessResult For(Word secret, Word guess)
    {
        var feedback = new LetterFeedback[Word.Length];
        var unmatchedSecretLetters = new Dictionary<char, int>();

        // Pass 1 — exact matches are CORRECT; tally the remaining secret letters.
        for (var i = 0; i < Word.Length; i++)
        {
            if (guess[i] == secret[i])
            {
                feedback[i] = LetterFeedback.Correct;
            }
            else
            {
                feedback[i] = LetterFeedback.Absent; // provisional, may become MISPLACED in pass 2
                unmatchedSecretLetters[secret[i]] = unmatchedSecretLetters.GetValueOrDefault(secret[i]) + 1;
            }
        }

        // Pass 2 — promote to MISPLACED only while an occurrence is still available.
        for (var i = 0; i < Word.Length; i++)
        {
            if (feedback[i] == LetterFeedback.Correct)
                continue;

            var letter = guess[i];
            if (unmatchedSecretLetters.GetValueOrDefault(letter) > 0)
            {
                feedback[i] = LetterFeedback.Misplaced;
                unmatchedSecretLetters[letter]--;
            }
        }

        var evaluations = new LetterEvaluation[Word.Length];
        for (var i = 0; i < Word.Length; i++)
            evaluations[i] = new LetterEvaluation(guess[i], feedback[i]);

        return new GuessResult(evaluations);
    }
}
