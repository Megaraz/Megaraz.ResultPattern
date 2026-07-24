namespace Megaraz.ResultPattern;

/// <summary>Represents the outcome of an operation without a return value.</summary>
public class Result
{
    private const string DefaultValidationFailureMessage =
        "Validation errors occurred, see validation errors for details.";

    /// <summary>Gets whether the operation completed successfully.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the result message. For failures this is normally suitable for presentation,
    /// while <see cref="PrimaryError.Description"/> remains technical diagnostic detail.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets validation errors. This collection is empty for successful and non-validation failures.
    /// </summary>
    public IReadOnlyCollection<ValidationError> ValidationErrors { get; }

    /// <summary>
    /// Gets the primary error. This is <see cref="Error.None"/> for successful results.
    /// For validation failures it is the first item in <see cref="ValidationErrors"/>.
    /// </summary>
    public Error PrimaryError { get; }

    internal Result(
        bool isSuccess,
        string message,
        IReadOnlyCollection<ValidationError> validationErrors,
        Error primaryError)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(validationErrors);
        ArgumentNullException.ThrowIfNull(primaryError);

        if (isSuccess)
        {
            if (primaryError.Type != ErrorType.None)
                throw new ArgumentException("A successful result cannot contain an error.", nameof(primaryError));
            if (validationErrors.Count != 0)
                throw new ArgumentException("A successful result cannot contain validation errors.", nameof(validationErrors));
        }
        else
        {
            if (primaryError.Type == ErrorType.None)
                throw new ArgumentException("A failed result must contain a primary error.", nameof(primaryError));

            if (primaryError.Type == ErrorType.Validation)
            {
                if (primaryError is not ValidationError validationPrimary)
                    throw new ArgumentException("A validation failure must have a ValidationError primary error.", nameof(primaryError));
                if (validationErrors.Count == 0 || !validationErrors.Contains(validationPrimary))
                    throw new ArgumentException("ValidationErrors must contain the primary validation error.", nameof(validationErrors));
            }
            else if (validationErrors.Count != 0)
            {
                throw new ArgumentException("Only validation failures may contain validation errors.", nameof(validationErrors));
            }
        }

        IsSuccess = isSuccess;
        Message = message;
        ValidationErrors = validationErrors.ToArray();
        PrimaryError = primaryError;
    }

    private Result() : this(true, string.Empty, Array.Empty<ValidationError>(), Error.None)
    {
    }

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new();

    /// <summary>Creates a validation failure containing at least one validation error.</summary>
    public static Result ValidationFailure(
        IEnumerable<ValidationError> validationErrors,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);
        var errors = validationErrors.ToArray();
        if (errors.Length == 0)
            throw new ArgumentException("A validation failure must contain at least one validation error.", nameof(validationErrors));

        return new(false, string.IsNullOrWhiteSpace(message) ? DefaultValidationFailureMessage : message, errors, errors[0]);
    }

    /// <summary>Creates a failure using the error's user-facing message.</summary>
    public static Result Failure(Error primaryError)
    {
        ArgumentNullException.ThrowIfNull(primaryError);
        return Failure(primaryError, primaryError.UserMessage);
    }

    /// <summary>Creates a failure with an explicit result message.</summary>
    public static Result Failure(Error primaryError, string message)
    {
        ArgumentNullException.ThrowIfNull(primaryError);
        ArgumentNullException.ThrowIfNull(message);
        return new(false, message, Array.Empty<ValidationError>(), primaryError);
    }
}

/// <summary>Represents the outcome of an operation with a non-null return value.</summary>
/// <typeparam name="TValue">The value type returned on success.</typeparam>
public sealed class Result<TValue> : Result where TValue : notnull
{
    private readonly TValue? _value;

    /// <summary>
    /// Gets the successful value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when this result is a failure.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    private Result(TValue value) : base(true, string.Empty, Array.Empty<ValidationError>(), Error.None)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    private Result(string message, IReadOnlyCollection<ValidationError> validationErrors, Error primaryError)
        : base(false, message, validationErrors, primaryError)
    {
    }

    /// <summary>Creates a successful result. Null values are not permitted.</summary>
    public static Result<TValue> Success(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new(value);
    }

    /// <summary>Creates a validation failure containing at least one validation error.</summary>
    public new static Result<TValue> ValidationFailure(
        IEnumerable<ValidationError> validationErrors,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(validationErrors);
        var errors = validationErrors.ToArray();
        if (errors.Length == 0)
            throw new ArgumentException("A validation failure must contain at least one validation error.", nameof(validationErrors));

        return new(
            string.IsNullOrWhiteSpace(message) ? "Validation errors occurred, see validation errors for details." : message,
            errors,
            errors[0]);
    }

    /// <summary>Creates a failure using the error's user-facing message.</summary>
    public new static Result<TValue> Failure(Error primaryError)
    {
        ArgumentNullException.ThrowIfNull(primaryError);
        return Failure(primaryError, primaryError.UserMessage);
    }

    /// <summary>Creates a failure with an explicit result message.</summary>
    public new static Result<TValue> Failure(Error primaryError, string message)
    {
        ArgumentNullException.ThrowIfNull(primaryError);
        ArgumentNullException.ThrowIfNull(message);
        return new(message, Array.Empty<ValidationError>(), primaryError);
    }

    internal static Result<TValue> FromFailure(
        string message,
        IReadOnlyCollection<ValidationError> validationErrors,
        Error primaryError) =>
        new(message, validationErrors, primaryError);
}
