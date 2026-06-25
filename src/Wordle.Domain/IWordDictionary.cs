namespace Wordle.Domain;

/// <summary>
/// The contract the game needs from a word source. The domain depends on this
/// abstraction only — it never knows whether the words come from a file, an API
/// or memory. Implementations live in the infrastructure layer; tests inject a double.
/// </summary>
public interface IWordDictionary
{
    /// <summary>Is this word an allowed guess?</summary>
    bool Contains(Word word);

    /// <summary>Choose the secret word for a new game.</summary>
    Word PickSecret();
}
