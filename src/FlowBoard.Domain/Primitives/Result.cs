namespace FlowBoard.Domain.Primitives;

/// <summary>
/// Represents the outcome of a domain operation without throwing exceptions
/// for expected failure paths. Use <see cref="Result{TValue}"/> when the
/// success case carries a value.
/// </summary>
public class Result
{
    /// <summary>Initialises the result.</summary>
    /// <param name="isSuccess"><c>true</c> for a successful outcome.</param>
    /// <param name="error">The error describing the failure. Must be <see cref="Error.None"/> on success.</param>
    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException("A successful result cannot carry an error.");

            case false when error == Error.None:
                throw new InvalidOperationException("A failed result must carry an error.");

            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    /// <summary><c>true</c> if the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary><c>true</c> if the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error that caused the failure. Equal to <see cref="Error.None"/> on success.
    /// </summary>
    public Error Error { get; }

    /// <summary>Creates a successful result with no return value.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result with the given error.</summary>
    /// <param name="error">The error describing the failure.</param>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a successful result carrying a value.</summary>
    /// <typeparam name="TValue">The type of the returned value.</typeparam>
    /// <param name="value">The value produced by the operation.</param>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>Creates a failed typed result with the given error.</summary>
    /// <typeparam name="TValue">The type that would have been returned on success.</typeparam>
    /// <param name="error">The error describing the failure.</param>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// A result that carries a typed value on success.
/// Access <see cref="Value"/> only after confirming <see cref="Result.IsSuccess"/> is <c>true</c>.
/// </summary>
/// <typeparam name="TValue">The type of value returned on success.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// The value produced by the operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");
}

/// <summary>Represents a domain-level error with a machine-readable code and human-readable description.</summary>
/// <param name="Code">A dot-separated error code (e.g. <c>"Task.NotFound"</c>).</param>
/// <param name="Description">A human-readable description of the error.</param>
public record Error(string Code, string Description)
{
    /// <summary>Represents the absence of an error. Used to indicate success.</summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>General error for unexpected null values.</summary>
    public static readonly Error NullValue = new("General.NullValue", "A null value was provided.");
}
