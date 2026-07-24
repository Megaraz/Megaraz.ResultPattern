namespace Megaraz.ResultPattern.Tests;

public class ErrorCodeTests
{
    [Fact]
    public void From_PreservesCallerOwnedCodeWithoutConvention()
    {
        var error = Error.Custom(
            ErrorCode.From("account/email-already-registered:v2"),
            "The email address is already registered.",
            ErrorType.Conflict);

        Assert.Equal("account/email-already-registered:v2", error.Code);
    }

    [Fact]
    public void CustomWithReason_UsesTheCallerDefinedReason()
    {
        var error = ValidationError.CustomWithReason(
            new ErrorContext(OperationType.Create, "User", "Email"),
            "EmailAlreadyRegistered",
            "The email address is already registered.");

        Assert.Equal("Create.User.EmailAlreadyRegistered", error.Code);
    }

    [Fact]
    public void BuiltInFactory_PreservesExistingConventionalCode()
    {
        var error = Error.NotFound(new ErrorContext(OperationType.Get, "User"));

        Assert.Equal("Get.User.NotFound", error.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid.reason")]
    public void For_RejectsMalformedReason(string reason)
    {
        var context = new ErrorContext(OperationType.Create, "User");

        Assert.Throws<ArgumentException>(() => ErrorCode.For(context, reason));
    }
}
