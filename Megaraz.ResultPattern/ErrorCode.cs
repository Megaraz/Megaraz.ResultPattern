namespace Megaraz.ResultPattern;

/// <summary>
/// Identifies a legacy built-in error reason.
/// </summary>
[Obsolete("Use ErrorCodeReasons or a caller-defined reason string instead. Enum member names are not error-code contracts.")]
public enum ErrorReasonCode
{
    /// <summary>A caller-defined reason.</summary>
    Custom = 0,
    /// <summary>A required value was missing.</summary>
    ValidationRequired = 100,
    /// <summary>A value had an invalid format.</summary>
    ValidationInvalidFormat = 101,
    /// <summary>A value was outside the allowed range.</summary>
    ValidationOutOfRange = 102,
    /// <summary>Two related values did not match.</summary>
    ValidationNonMatchingValues = 103,
    /// <summary>A value was shorter than allowed.</summary>
    ValidationTooShort = 104,
    /// <summary>A value was longer than allowed.</summary>
    ValidationTooLong = 105,
    /// <summary>A value conflicted with an existing value.</summary>
    ValidationAlreadyExists = 106,
    /// <summary>An operation was cancelled.</summary>
    OperationCancelled = 250,
    /// <summary>An unexpected failure occurred.</summary>
    GeneralFailure = 300,
    /// <summary>A requested resource was not found.</summary>
    GeneralNotFound = 301,
    /// <summary>A request conflicted with the current state.</summary>
    GeneralConflict = 302,
    /// <summary>The caller was not authenticated.</summary>
    GeneralUnauthorized = 303,
    /// <summary>The caller was not permitted to perform the operation.</summary>
    GeneralForbidden = 304
}

/// <summary>Stable reason strings used by the built-in error factories.</summary>
public static class ErrorCodeReasons
{
    /// <summary>A required value was missing.</summary>
    public const string Required = "Required";
    /// <summary>A value had an invalid format.</summary>
    public const string InvalidFormat = "InvalidFormat";
    /// <summary>A value was outside the allowed range.</summary>
    public const string OutOfRange = "OutOfRange";
    /// <summary>Two related values did not match.</summary>
    public const string NonMatchingValues = "NonMatchingValues";
    /// <summary>A value was shorter than allowed.</summary>
    public const string TooShort = "TooShort";
    /// <summary>A value was longer than allowed.</summary>
    public const string TooLong = "TooLong";
    /// <summary>A value conflicted with an existing value.</summary>
    public const string AlreadyExists = "AlreadyExists";
    /// <summary>An operation was cancelled.</summary>
    public const string Cancelled = "Cancelled";
    /// <summary>An unexpected failure occurred.</summary>
    public const string Failure = "Failure";
    /// <summary>A requested resource was not found.</summary>
    public const string NotFound = "NotFound";
    /// <summary>A request conflicted with the current state.</summary>
    public const string Conflict = "Conflict";
    /// <summary>The caller was not authenticated.</summary>
    public const string Unauthorized = "Unauthorized";
    /// <summary>The caller was not permitted to perform the operation.</summary>
    public const string Forbidden = "Forbidden";
}

/// <summary>
/// A stable, programmatic error identifier.
/// </summary>
/// <remarks>
/// Use <see cref="From"/> for a caller-owned code. <see cref="For(ErrorContext, string)"/>
/// is available when the conventional <c>Operation.Entity.Reason</c> shape is useful,
/// but it is never required.
/// </remarks>
public sealed record ErrorCode
{
    private ErrorCode(string code)
    {
        Code = code;
    }

    /// <summary>Gets the exact stable code value.</summary>
    public string Code { get; }

    /// <summary>Creates a caller-owned error code without imposing a format.</summary>
    public static ErrorCode From(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null, empty, or whitespace.", nameof(code));

        return new(code);
    }

    /// <summary>Creates a conventional error code from a context and caller-defined reason.</summary>
    public static ErrorCode For(ErrorContext errorContext, string reason)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        ValidatePart(errorContext.EntityName, nameof(errorContext));
        ValidatePart(reason, nameof(reason));
        return From($"{GetOperationPart(errorContext.Operation)}.{errorContext.EntityName}.{reason}");
    }

    /// <summary>Creates a conventional error code from a legacy built-in reason.</summary>
    [Obsolete("Use ErrorCode.For(errorContext, ErrorCodeReasons...) or a caller-defined reason string instead.")]
    public static ErrorCode For(ErrorContext errorContext, ErrorReasonCode reason) =>
        For(errorContext, reason.ToCodePart());

    private static void ValidatePart(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('.'))
            throw new ArgumentException("Code parts cannot be null, empty, whitespace, or contain '.'.", parameterName);
    }

    private static string GetOperationPart(OperationType operation) => operation switch
    {
        OperationType.Custom => "Custom",
        OperationType.Create => "Create",
        OperationType.Get => "Get",
        OperationType.GetCollection => "GetCollection",
        OperationType.Update => "Update",
        OperationType.Delete => "Delete",
        OperationType.Login => "Login",
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, "The operation must be a defined value.")
    };
}
