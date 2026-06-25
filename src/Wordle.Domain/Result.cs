namespace Wordle.Domain;

/// <summary>
/// Result of an operation that can fail for a known business reason.
/// Either a success carrying a <typeparamref name="TValue"/>, or a failure
/// carrying a <typeparamref name="TError"/>. Expected domain failures are
/// modelled as values here rather than thrown as exceptions.
/// </summary>
public readonly record struct Result<TValue, TError>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private readonly TValue _value;
    private readonly TError _error;

    private Result(bool isSuccess, TValue value, TError error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static Result<TValue, TError> Success(TValue value) => new(true, value, default!);

    public static Result<TValue, TError> Failure(TError error) => new(false, default!, error);

    public TValue Value => IsSuccess
        ? _value
        : throw new InvalidOperationException("Cannot read Value of a failed result.");

    public TError Error => IsFailure
        ? _error
        : throw new InvalidOperationException("Cannot read Error of a successful result.");
}
