using Xunit;

namespace Wordle.Domain.Tests;

public class GameTests
{
    [Fact]
    public void A_new_game_is_in_progress_with_six_attempts()
    {
        // Given a dictionary whose secret is LIVRE
        var dictionary = new FakeWordDictionary(secret: AsWord("LIVRE"));

        // When a new game starts
        var game = new Game(dictionary);

        // Then it is in progress, with all six attempts and an empty history
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(6, game.RemainingAttempts);
        Assert.Empty(game.History);
    }

    [Fact]
    public void Submitting_a_valid_non_winning_guess_returns_feedback_and_consumes_one_attempt()
    {
        // Given a game in progress where RAMER is a valid (wrong) word
        var dictionary = new FakeWordDictionary(secret: AsWord("LIVRE"), AsWord("RAMER"));
        var game = new Game(dictionary);

        // When the player submits a valid word that is not the secret
        var outcome = game.Play(AsWord("RAMER"));

        // Then the feedback is returned, one attempt is consumed, the game stays in progress
        Assert.True(outcome.IsSuccess);
        Assert.False(outcome.Value.IsWin);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(5, game.RemainingAttempts);
        Assert.Single(game.History);
    }

    [Fact]
    public void Guessing_the_secret_word_wins_the_game()
    {
        // Given a game in progress
        var dictionary = new FakeWordDictionary(secret: AsWord("LIVRE"));
        var game = new Game(dictionary);

        // When the player submits the secret word
        var outcome = game.Play(AsWord("LIVRE"));

        // Then the guess wins and the game becomes Won
        Assert.True(outcome.IsSuccess);
        Assert.True(outcome.Value.IsWin);
        Assert.Equal(GameStatus.Won, game.Status);
    }

    [Fact]
    public void Exhausting_the_six_attempts_without_finding_loses_the_game()
    {
        // Given a game whose secret is LIVRE and RAMER is a valid (wrong) word
        var dictionary = new FakeWordDictionary(secret: AsWord("LIVRE"), AsWord("RAMER"));
        var game = new Game(dictionary);

        // When the player submits six valid but wrong guesses
        for (var attempt = 0; attempt < 6; attempt++)
            game.Play(AsWord("RAMER"));

        // Then the game is Lost with no attempts left
        Assert.Equal(GameStatus.Lost, game.Status);
        Assert.Equal(0, game.RemainingAttempts);
    }

    [Fact]
    public void Finding_the_secret_on_the_sixth_attempt_wins_rather_than_loses()
    {
        // Given a game with five wrong guesses already played
        var dictionary = new FakeWordDictionary(secret: AsWord("LIVRE"), AsWord("RAMER"));
        var game = new Game(dictionary);
        for (var attempt = 0; attempt < 5; attempt++)
            game.Play(AsWord("RAMER"));

        // When the sixth and last guess is the secret word
        var outcome = game.Play(AsWord("LIVRE"));

        // Then victory takes precedence over running out of attempts
        Assert.True(outcome.Value.IsWin);
        Assert.Equal(GameStatus.Won, game.Status);
    }

    [Fact]
    public void A_guess_outside_the_dictionary_is_rejected_without_consuming_an_attempt()
    {
        // Given a game in progress; ZZZZZ is well-formed but not in the dictionary
        var dictionary = new FakeWordDictionary(secret: AsWord("LIVRE"));
        var game = new Game(dictionary);

        // When the player submits a word that is not in the dictionary
        var outcome = game.Play(AsWord("ZZZZZ"));

        // Then it is rejected, no attempt is consumed and the game is unchanged
        Assert.True(outcome.IsFailure);
        Assert.Equal(PlayError.NotInDictionary, outcome.Error);
        Assert.Equal(6, game.RemainingAttempts);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Empty(game.History);
    }

    [Fact]
    public void Playing_after_the_game_is_won_is_rejected()
    {
        // Given a game that has already been won
        var dictionary = new FakeWordDictionary(secret: AsWord("LIVRE"), AsWord("RAMER"));
        var game = new Game(dictionary);
        game.Play(AsWord("LIVRE"));

        // When the player tries to play again
        var outcome = game.Play(AsWord("RAMER"));

        // Then it is rejected because the game is over, and nothing changes
        Assert.True(outcome.IsFailure);
        Assert.Equal(PlayError.GameOver, outcome.Error);
        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Single(game.History);
    }

    [Fact]
    public void Playing_after_the_game_is_lost_is_rejected()
    {
        // Given a game that has already been lost
        var dictionary = new FakeWordDictionary(secret: AsWord("LIVRE"), AsWord("RAMER"));
        var game = new Game(dictionary);
        for (var attempt = 0; attempt < 6; attempt++)
            game.Play(AsWord("RAMER"));

        // When the player tries to play again
        var outcome = game.Play(AsWord("RAMER"));

        // Then it is rejected because the game is over
        Assert.True(outcome.IsFailure);
        Assert.Equal(PlayError.GameOver, outcome.Error);
        Assert.Equal(GameStatus.Lost, game.Status);
    }

    private static Word AsWord(string raw) => Word.Create(raw).Value;
}
