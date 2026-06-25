using Xunit;

namespace Wordle.Domain.Tests;

public class WordTests
{
    [Fact]
    public void A_five_letter_string_creates_a_valid_word()
    {
        // Given a five-letter string
        var raw = "LIVRE";

        // When we create a Word from it
        var result = Word.Create(raw);

        // Then the creation succeeds and the word exposes its letters
        Assert.True(result.IsSuccess);
        Assert.Equal("LIVRE", result.Value.Text);
    }

    [Fact]
    public void A_word_is_normalized_to_uppercase()
    {
        // Given a lowercase five-letter string
        var raw = "livre";

        // When we create a Word from it
        var result = Word.Create(raw);

        // Then the stored letters are uppercased (Wordle is case-insensitive)
        Assert.True(result.IsSuccess);
        Assert.Equal("LIVRE", result.Value.Text);
    }

    [Theory]
    [InlineData("LIVR")]    // four letters
    [InlineData("LIVRES")]  // six letters
    [InlineData("")]        // empty
    public void A_string_that_is_not_five_letters_is_rejected(string raw)
    {
        // When we create a Word from a string of the wrong length
        var result = Word.Create(raw);

        // Then it is rejected because of its length
        Assert.True(result.IsFailure);
        Assert.Equal(WordFormatError.WrongLength, result.Error);
    }

    [Theory]
    [InlineData("LIV2E")]   // contains a digit
    [InlineData("LI-RE")]   // contains a symbol
    [InlineData("LIV E")]   // contains a space
    public void A_five_character_string_with_non_letters_is_rejected(string raw)
    {
        // When we create a Word containing non-letter characters
        var result = Word.Create(raw);

        // Then it is rejected because it is not alphabetic
        Assert.True(result.IsFailure);
        Assert.Equal(WordFormatError.NotAlphabetic, result.Error);
    }

    [Fact]
    public void Two_words_with_the_same_letters_are_equal()
    {
        // Given two words built from the same letters in different cases
        var secretWord = Word.Create("LIVRE").Value;
        var playerGuess = Word.Create("livre").Value;

        // Then they are considered equal (value-object semantics)
        Assert.Equal(secretWord, playerGuess);
    }
}
