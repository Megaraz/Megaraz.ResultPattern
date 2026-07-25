namespace Megaraz.ResultPattern.Tests.Error;

public class ErrorReasonCodeCompatibilityTests
{
#pragma warning disable CS0618
    [Theory]
    [InlineData(ErrorReasonCode.Custom, "Custom")]
    [InlineData(ErrorReasonCode.ValidationRequired, "Required")]
    [InlineData(ErrorReasonCode.ValidationInvalidFormat, "InvalidFormat")]
    [InlineData(ErrorReasonCode.ValidationOutOfRange, "OutOfRange")]
    [InlineData(ErrorReasonCode.ValidationNonMatchingValues, "NonMatchingValues")]
    [InlineData(ErrorReasonCode.ValidationTooShort, "TooShort")]
    [InlineData(ErrorReasonCode.ValidationTooLong, "TooLong")]
    [InlineData(ErrorReasonCode.ValidationAlreadyExists, "AlreadyExists")]
    [InlineData(ErrorReasonCode.OperationCancelled, "Cancelled")]
    [InlineData(ErrorReasonCode.GeneralFailure, "Failure")]
    [InlineData(ErrorReasonCode.GeneralNotFound, "NotFound")]
    [InlineData(ErrorReasonCode.GeneralConflict, "Conflict")]
    [InlineData(ErrorReasonCode.GeneralUnauthorized, "Unauthorized")]
    [InlineData(ErrorReasonCode.GeneralForbidden, "Forbidden")]
    public void LegacyReasonProducesModernCode(ErrorReasonCode reason, string reasonPart)
    {
        var code = ErrorCode.For(TestData.Context, reason);
        Assert.Equal($"Create.User.{reasonPart}", code.Code);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(305)]
    public void UndefinedLegacyReasonThrows(int value)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => ErrorCode.For(TestData.Context, (ErrorReasonCode)value));
        Assert.Equal("reason", ex.ParamName);
    }
#pragma warning restore CS0618
}
