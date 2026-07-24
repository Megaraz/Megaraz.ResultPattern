namespace Megaraz.ResultPattern;

/// <summary>Identifies the common category of a validation error.</summary>
public enum ValidationErrorType
{
    Custom = 0,
    Required = 1,
    InvalidFormat = 2,
    OutOfRange = 3,
    NonMatchingValues = 4,
    AlreadyExists = 5,
    TooShort = 6,
    TooLong = 7
}

/// <summary>An error caused by invalid input, optionally associated with a field or property.</summary>
public sealed record ValidationError : Error
{
    /// <summary>The common validation category.</summary>
    public ValidationErrorType ValidationErrorType { get; }

    /// <summary>The field or property associated with the error, or <see langword="null"/> for a general error.</summary>
    public string? FieldName { get; }

    private ValidationError(
        string code,
        string description,
        ValidationErrorType type,
        string userMessage,
        string? fieldName)
        : base(code, description, ErrorType.Validation, userMessage)
    {
        ValidationErrorType = type;
        FieldName = fieldName;
    }

    /// <summary>Creates a custom validation error with caller-provided wording.</summary>
    public static ValidationError Custom(
        ErrorContext errorContext,
        string description = "A custom validation error occurred.",
        string? userMessage = null,
        string? fieldName = null)
    {
        ArgumentNullException.ThrowIfNull(errorContext);
        var code = ErrorCode.For(errorContext, ErrorReasonCode.Custom);
        var resolvedFieldName = fieldName ?? errorContext.FieldName;
        return new(code.Code, FormatDescription(errorContext, description), ValidationErrorType.Custom,
            userMessage ?? description, resolvedFieldName);
    }

    /// <summary>Creates a required-value validation error.</summary>
    public static ValidationError Required(ErrorContext errorContext, string? userMessage = null) =>
        Create(errorContext, ErrorReasonCode.ValidationRequired, ValidationErrorType.Required,
            string.IsNullOrWhiteSpace(errorContext.FieldName)
                ? $"A value for '{errorContext.EntityName}' is required."
                : $"A value for '{errorContext.FieldName}' is required.",
            userMessage);

    /// <summary>Creates an invalid-format validation error.</summary>
    public static ValidationError InvalidFormat(ErrorContext errorContext, string expectedFormat, string? userMessage = null) =>
        Create(errorContext, ErrorReasonCode.ValidationInvalidFormat, ValidationErrorType.InvalidFormat,
            $"The field '{errorContext.FieldName}' has an invalid format. Expected format: {expectedFormat}.", userMessage);

    /// <summary>Creates an out-of-range validation error.</summary>
    public static ValidationError OutOfRange(ErrorContext errorContext, string range, string? userMessage = null) =>
        Create(errorContext, ErrorReasonCode.ValidationOutOfRange, ValidationErrorType.OutOfRange,
            $"The field '{errorContext.FieldName}' is out of range. Expected range: {range}.", userMessage);

    /// <summary>Creates a minimum-length validation error.</summary>
    public static ValidationError TooShort(ErrorContext errorContext, string range, string? userMessage = null) =>
        Create(errorContext, ErrorReasonCode.ValidationTooShort, ValidationErrorType.TooShort,
            $"The field '{errorContext.FieldName}' is too short. Expected minimum length: {range}.", userMessage);

    /// <summary>Creates a maximum-length validation error.</summary>
    public static ValidationError TooLong(ErrorContext errorContext, string range, string? userMessage = null) =>
        Create(errorContext, ErrorReasonCode.ValidationTooLong, ValidationErrorType.TooLong,
            $"The field '{errorContext.FieldName}' is too long. Expected maximum length: {range}.", userMessage);

    /// <summary>Creates an already-exists validation error.</summary>
    public static ValidationError AlreadyExists(ErrorContext errorContext, string? userMessage = null) =>
        Create(errorContext, ErrorReasonCode.ValidationAlreadyExists, ValidationErrorType.AlreadyExists,
            $"A {errorContext.EntityName} with that {errorContext.FieldName} already exists.", userMessage);

    /// <summary>Creates a validation error for two values that do not match.</summary>
    public static ValidationError NonMatchingValues(ErrorContext errorContext, string? confirmFieldName = null, string? userMessage = null)
    {
        var message = !string.IsNullOrWhiteSpace(errorContext.FieldName) && !string.IsNullOrWhiteSpace(confirmFieldName)
            ? $"The values for '{errorContext.FieldName}' and '{confirmFieldName}' do not match."
            : "The provided values do not match.";
        return Create(errorContext with { FieldName = confirmFieldName ?? errorContext.FieldName },
            ErrorReasonCode.ValidationNonMatchingValues, ValidationErrorType.NonMatchingValues, message, userMessage);
    }

    private static ValidationError Create(
        ErrorContext context,
        ErrorReasonCode reason,
        ValidationErrorType type,
        string message,
        string? userMessage)
    {
        ArgumentNullException.ThrowIfNull(context);
        var code = ErrorCode.For(context, reason);
        return new(code.Code, FormatDescription(context, message), type, userMessage ?? message, context.FieldName);
    }
}