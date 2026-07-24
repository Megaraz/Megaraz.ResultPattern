namespace Megaraz.ResultPattern;

#pragma warning disable CS0618 // Maintains the obsolete ErrorReasonCode compatibility adapter.
internal static class ErrorReasonCodeExtensions
{
    public static string ToCodePart(this ErrorReasonCode reason) => reason switch
    {
        ErrorReasonCode.ValidationRequired => "Required",
        ErrorReasonCode.ValidationInvalidFormat => "InvalidFormat",
        ErrorReasonCode.ValidationOutOfRange => "OutOfRange",
        ErrorReasonCode.ValidationNonMatchingValues => "NonMatchingValues",
        ErrorReasonCode.ValidationTooShort => "TooShort",
        ErrorReasonCode.ValidationTooLong => "TooLong",
        ErrorReasonCode.ValidationAlreadyExists => "AlreadyExists",
        ErrorReasonCode.OperationCancelled => "Cancelled",
        ErrorReasonCode.GeneralFailure => "Failure",
        ErrorReasonCode.GeneralNotFound => "NotFound",
        ErrorReasonCode.GeneralConflict => "Conflict",
        ErrorReasonCode.GeneralUnauthorized => "Unauthorized",
        ErrorReasonCode.GeneralForbidden => "Forbidden",
        ErrorReasonCode.Custom => "Custom",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
    };
}
#pragma warning restore CS0618
