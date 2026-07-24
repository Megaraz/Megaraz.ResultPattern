namespace Megaraz.ResultPattern;

/// <summary>Identifies the reason an error occurred.</summary>
public enum ErrorReasonCode
{
    Custom = 0,
    ValidationRequired = 100,
    ValidationInvalidFormat = 101,
    ValidationOutOfRange = 102,
    ValidationNonMatchingValues = 103,
    ValidationTooShort = 104,
    ValidationTooLong = 105,
    ValidationAlreadyExists = 106,
    OperationCancelled = 250,
    GeneralFailure = 300,
    GeneralNotFound = 301,
    GeneralConflict = 302,
    GeneralUnauthorized = 303,
    GeneralForbidden = 304
}

/// <summary>
/// A structured, stable identifier composed of an operation, entity, and reason.
/// </summary>
public sealed record ErrorCode
{
    /// <summary>The operation associated with the error.</summary>
    public OperationType Operation { get; }

    /// <summary>The logical entity or resource associated with the error.</summary>
    public string NameOfEntity { get; }

    /// <summary>The structured reason for the error.</summary>
    public ErrorReasonCode Reason { get; }

    /// <summary>The formatted code intended for programmatic identification and logging.</summary>
    public string Code => $"{Operation}.{NameOfEntity}.{Reason.ToCodePart()}";

    private ErrorCode(OperationType operation, string nameOfEntity, ErrorReasonCode reason)
    {
        Operation = operation;
        NameOfEntity = nameOfEntity;
        Reason = reason;
    }

    /// <summary>Creates an error code from an error context and reason.</summary>
    public static ErrorCode For(ErrorContext errorContext, ErrorReasonCode reason)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        return new(errorContext.Operation, errorContext.EntityName, reason);
    }
}
