using Xunit;

namespace Wordle.Domain.Tests;

public class GuessResultTests
{
    [Fact]
    public void Guessing_the_secret_word_yields_all_correct_and_is_a_win()
    {
        // Given the secret word
        var secretWord = AsWord("LIVRE");

        // When the exact same word is submitted
        var result = GuessResult.For(secretWord, AsWord("LIVRE"));

        // Then every letter is CORRECT and the guess wins the game
        Assert.All(result.Letters, letter => Assert.Equal(LetterFeedback.Correct, letter.Feedback));
        Assert.True(result.IsWin);
    }

    [Fact]
    public void A_guess_different_from_the_secret_is_not_a_win()
    {
        // Given a guess that is not the secret
        var secretWord = AsWord("LIVRE");

        // When it is evaluated
        var result = GuessResult.For(secretWord, AsWord("RAMER"));

        // Then it is not a winning guess
        Assert.False(result.IsWin);
    }

    [Theory]
    [InlineData("LIVRE", "RIVAL", "MCCAM")] // each feedback type appears at least once
    [InlineData("LIVRE", "RAMER", "MAAMA")] // énoncé case: the 2nd R is ABSENT (single R already used)
    [InlineData("LIVRE", "EAGER", "MAAAM")] // two E in the guess, one E in the secret
    [InlineData("ALERT", "EVENT", "AACAC")] // the CORRECT E consumes the only E -> the other E is ABSENT
    public void A_guess_is_evaluated_letter_by_letter_respecting_multiple_letters(
        string secret, string guess, string expectedPattern)
    {
        // Given a secret word and a player's guess
        var secretWord = AsWord(secret);
        var playerGuess = AsWord(guess);

        // When the guess is evaluated against the secret
        var result = GuessResult.For(secretWord, playerGuess);

        // Then each position receives the feedback required by the rules
        Assert.Equal(Feedbacks(expectedPattern), result.Letters.Select(l => l.Feedback));
    }

    [Fact]
    public void Each_evaluation_keeps_the_letter_that_was_guessed()
    {
        // Given a guess
        var result = GuessResult.For(AsWord("LIVRE"), AsWord("RAMER"));

        // Then the result reports the guessed letters (so the UI can display them)
        var guessedLetters = new string(result.Letters.Select(l => l.Letter).ToArray());
        Assert.Equal("RAMER", guessedLetters);
    }

    private static Word AsWord(string raw) => Word.Create(raw).Value;

    private static LetterFeedback[] Feedbacks(string pattern) =>
        pattern.Select(code => code switch
        {
            'C' => LetterFeedback.Correct,
            'M' => LetterFeedback.Misplaced,
            'A' => LetterFeedback.Absent,
            _ => throw new ArgumentException($"Unknown feedback code '{code}'")
        }).ToArray();
}
