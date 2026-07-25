namespace Megaraz.ResultPattern;

/// <summary>Classifies the semantic kind of an <see cref="Error"/>.</summary>
public enum ErrorType
{
    /// <summary>No error.</summary>
    None = 0,
    /// <summary>A general operation failure.</summary>
    Failure = 1,
    /// <summary>An input validation failure.</summary>
    Validation = 2,
    /// <summary>A requested resource was not found.</summary>
    NotFound = 3,
    /// <summary>A request conflicted with the current state.</summary>
    Conflict = 4,
    /// <summary>The caller was not authenticated.</summary>
    Unauthorized = 5,
    /// <summary>The caller was not authorized.</summary>
    Forbidden = 6,
    /// <summary>An operation was cancelled.</summary>
    Cancelled = 7
}

/// <summary>
/// Represents an operation error with a technical description and an optional user-facing message.
/// </summary>
public record Error
{
    /// <summary>
    /// Initializes a normal error. Public callers should use a custom or convenience factory.
    /// </summary>
    protected Error(
        string code,
        string description,
        ErrorType type,
        string userMessage = "",
        Exception? exception = null)
        : this(code, description, type, userMessage, exception, false)
    {
    }

    private Error(
        string code,
        string description,
        ErrorType type,
        string userMessage,
        Exception? exception,
        bool allowNone)
    {
        if (!allowNone)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Error code cannot be null, empty, or whitespace.", nameof(code));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Error description cannot be null, empty, or whitespace.", nameof(description));
            if (!Enum.IsDefined(type))
                throw new ArgumentOutOfRangeException(nameof(type), type, "The error type must be a defined value.");
            if (type == ErrorType.None)
                throw new ArgumentException("ErrorType.None is reserved for Error.None.", nameof(type));
            if (type == ErrorType.Validation && this is not ValidationError)
                throw new ArgumentException("Validation errors must be created with ValidationError.", nameof(type));
        }

        Code = code;
        Description = description;
        Type = type;
        UserMessage = userMessage ?? string.Empty;
        Exception = exception;
    }

    /// <summary>Creates a normal error from caller-provided values.</summary>
    public static Error Custom(
        string code,
        string description,
        ErrorType type,
        string? userMessage = null,
        Exception? exception = null)
    {
        if (type == ErrorType.Validation)
            throw new ArgumentException("Validation errors must be created with ValidationError.", nameof(type));

        return new(code, description, type, userMessage ?? string.Empty, exception, false);
    }

    /// <summary>Creates a normal error from a caller-owned error code.</summary>
    public static Error Custom(
        ErrorCode code,
        string description,
        ErrorType type,
        string? userMessage = null,
        Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        return Custom(code.Code, description, type, userMessage, exception);
    }

    /// <summary>Creates a normal error using the optional conventional code shape.</summary>
    public static Error Custom(
        ErrorContext errorContext,
        string reason,
        ErrorType type,
        string description,
        string? userMessage = null,
        Exception? exception = null) =>
        Custom(ErrorCode.For(errorContext, reason), description, type, userMessage, exception);

    /// <summary>Gets the stable, programmatic error code.</summary>
    public string Code { get; }

    /// <summary>Gets technical information describing what went wrong.</summary>
    public string Description { get; }

    /// <summary>Gets the semantic category of the error.</summary>
    public ErrorType Type { get; }

    /// <summary>Gets an optional user-facing message.</summary>
    public string UserMessage { get; }

    /// <summary>Gets the exception associated with the error, when available.</summary>
    public Exception? Exception { get; }

    /// <summary>Represents the absence of an error on a successful result.</summary>
    public static readonly Error None = new(
        string.Empty,
        string.Empty,
        ErrorType.None,
        string.Empty,
        null,
        true);

    /// <summary>Creates a not-found error.</summary>
    public static Error NotFound(
        ErrorContext errorContext,
        string? userMessage = null,
        Func<ErrorContext, string>? messageFactory = null)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        var message = $"{errorContext.EntityName} not found";
        return Create(errorContext, ErrorCodeReasons.NotFound, ErrorType.NotFound, message, userMessage: userMessage,
            messageFactory: messageFactory);
    }

    /// <summary>Creates a conflict error.</summary>
    public static Error Conflict(
        ErrorContext errorContext,
        string? userMessage = null,
        Func<ErrorContext, string>? messageFactory = null)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        var message = $"Unique {errorContext.EntityName} constraint violated.";
        return Create(errorContext, ErrorCodeReasons.Conflict, ErrorType.Conflict, message, userMessage: userMessage,
            messageFactory: messageFactory);
    }

    /// <summary>Creates an unauthorized error.</summary>
    public static Error Unauthorized(
        ErrorContext errorContext,
        string? userMessage = null,
        Func<ErrorContext, string>? messageFactory = null)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        var message = "Unauthorized access" +
            (string.IsNullOrWhiteSpace(errorContext.FieldName) ? string.Empty : $" to {errorContext.FieldName}");
        return Create(errorContext, ErrorCodeReasons.Unauthorized, ErrorType.Unauthorized, message, userMessage: userMessage,
            messageFactory: messageFactory);
    }

    /// <summary>Creates a forbidden error.</summary>
    public static Error Forbidden(
        ErrorContext errorContext,
        string? userMessage = null,
        Func<ErrorContext, string>? messageFactory = null)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        return Create(errorContext, ErrorCodeReasons.Forbidden, ErrorType.Forbidden, "Forbidden access.",
            userMessage: userMessage, messageFactory: messageFactory);
    }

    /// <summary>Creates a general failure error.</summary>
    public static Error Failure(
        ErrorContext errorContext,
        string? description = null,
        Exception? exception = null,
        string? userMessage = null,
        Func<ErrorContext, string>? messageFactory = null)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        var message = string.IsNullOrWhiteSpace(description)
            ? $"An unexpected failure occurred while processing {errorContext.EntityName}."
            : description;
        return Create(errorContext, ErrorCodeReasons.Failure, ErrorType.Failure, message, exception, userMessage,
            messageFactory);
    }

    /// <summary>Creates an operation-cancelled error.</summary>
    public static Error Cancelled(
        ErrorContext errorContext,
        string? userMessage = null,
        Func<ErrorContext, string>? messageFactory = null)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        var message = $"The operation on {errorContext.EntityName} was cancelled.";
        return Create(errorContext, ErrorCodeReasons.Cancelled, ErrorType.Cancelled, message, userMessage: userMessage,
            messageFactory: messageFactory);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"Error Code: {Code}{Environment.NewLine}Description: {Description}";

    /// <summary>
    /// Returns the technical description without imposing an application-specific logging format.
    /// </summary>
    protected static string FormatDescription(ErrorContext errorContext, string description)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        return description;
    }

    private static Error Create(
        ErrorContext context,
        string reason,
        ErrorType type,
        string message,
        Exception? exception = null,
        string? userMessage = null,
        Func<ErrorContext, string>? messageFactory = null)
    {
        var code = ErrorCode.For(context, reason);
        var description = messageFactory is null ? message : messageFactory(context);
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("The message factory must return a non-blank message.", nameof(messageFactory));

        return Custom(code.Code, FormatDescription(context, description), type, userMessage ?? string.Empty, exception);
    }
}