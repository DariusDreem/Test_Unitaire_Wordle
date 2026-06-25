namespace Wordle.Web;

/// <summary>JSON shapes exchanged with the browser. The secret is only ever sent on a loss.</summary>
public record NewGameResponse(Guid Id, int MaxAttempts, int WordLength, string Status, int RemainingAttempts);

public record GuessRequest(string? Word);

public record GuessResponse(
    bool Accepted,
    string? Reason,
    IReadOnlyList<string>? Feedback,
    bool IsWin,
    string Status,
    int RemainingAttempts,
    string? Secret);
