namespace Megaraz.ResultPattern;

/// <summary>Classifies the semantic kind of an <see cref="Error"/>.</summary>
public enum ErrorType
{
    None = 0,
    Failure = 1,
    Validation = 2,
    NotFound = 3,
    Conflict = 4,
    Unauthorized = 5,
    Forbidden = 6,
    Cancelled = 7
}

/// <summary>
/// Represents an operation error with a technical description and an optional user-facing message.
/// </summary>
public record Error(
    string Code,
    string Description,
    ErrorType Type,
    string UserMessage = "",
    Exception? Exception = null)
{
    /// <summary>Represents the absence of an error on a successful result.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>Creates a not-found error.</summary>
    public static Error NotFound(ErrorContext errorContext)
    {
        var message = $"{errorContext.EntityName} not found";
        return Create(errorContext, ErrorReasonCode.GeneralNotFound, ErrorType.NotFound, message);
    }

    /// <summary>Creates a conflict error.</summary>
    public static Error Conflict(ErrorContext errorContext)
    {
        var message = $"Unique {errorContext.EntityName} constraint violated.";
        return Create(errorContext, ErrorReasonCode.GeneralConflict, ErrorType.Conflict, message);
    }

    /// <summary>Creates an unauthorized error.</summary>
    public static Error Unauthorized(ErrorContext errorContext)
    {
        var message = "Unauthorized access" +
            (string.IsNullOrWhiteSpace(errorContext.FieldName) ? string.Empty : $" to {errorContext.FieldName}");
        return Create(errorContext, ErrorReasonCode.GeneralUnauthorized, ErrorType.Unauthorized, message);
    }

    /// <summary>Creates a forbidden error.</summary>
    public static Error Forbidden(ErrorContext errorContext)
    {
        return Create(errorContext, ErrorReasonCode.GeneralForbidden, ErrorType.Forbidden, "Forbidden access.");
    }

    /// <summary>Creates a general failure error.</summary>
    public static Error Failure(
        ErrorContext errorContext,
        string? description = null,
        Exception? exception = null,
        string? userMessage = null)
    {
        var message = string.IsNullOrWhiteSpace(description)
            ? $"An unexpected failure occurred while processing {errorContext.EntityName}."
            : description;
        return Create(errorContext, ErrorReasonCode.GeneralFailure, ErrorType.Failure, message, exception, userMessage);
    }

    /// <summary>Creates an operation-cancelled error.</summary>
    public static Error Cancelled(ErrorContext errorContext)
    {
        var message = $"The operation on {errorContext.EntityName} was cancelled.";
        return Create(errorContext, ErrorReasonCode.OperationCancelled, ErrorType.Cancelled, message);
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"Error Code: {Code}{Environment.NewLine}Description: {Description}";

    /// <summary>Formats technical context for a diagnostic description.</summary>
    protected static string FormatDescription(ErrorContext errorContext, string description)
    {
        return $"An error occurred during {errorContext.Operation} on entity {errorContext.EntityName}:{Environment.NewLine}" +
               $"Layer: {errorContext.Layer}{Environment.NewLine}" +
               $"Service: {errorContext.ServiceName}{Environment.NewLine}" +
               $"Method: {errorContext.MethodName}: {description}";
    }

    private static Error Create(
        ErrorContext context,
        ErrorReasonCode reason,
        ErrorType type,
        string message,
        Exception? exception = null,
        string? userMessage = null)
    {
        var code = ErrorCode.For(context, reason);
        return new(code.Code, FormatDescription(context, message), type, userMessage ?? message, exception);
    }
}