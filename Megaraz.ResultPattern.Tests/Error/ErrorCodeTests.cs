namespace Megaraz.ResultPattern.Tests.Error;

public class ErrorCodeTests
{
    [Theory]
    [InlineData("account/email:v2")]
    [InlineData("not.conventional")]
    public void From_PreservesCallerOwnedCode(string code) => Assert.Equal(code, ErrorCode.From(code).Code);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void From_RejectsBlankCode(string? code)
    {
        var ex = Assert.Throws<ArgumentException>(() => ErrorCode.From(code!));
        Assert.Equal("code", ex.ParamName);
    }

    [Theory]
    [InlineData(OperationType.Custom, "Custom")]
    [InlineData(OperationType.Create, "Create")]
    [InlineData(OperationType.Get, "Get")]
    [InlineData(OperationType.GetCollection, "GetCollection")]
    [InlineData(OperationType.Update, "Update")]
    [InlineData(OperationType.Delete, "Delete")]
    [InlineData(OperationType.Login, "Login")]
    public void For_UsesOperationEntityAndReason(OperationType operation, string operationPart)
    {
        var code = ErrorCode.For(new ErrorContext(operation, "User"), ErrorCodeReasons.Required);
        Assert.Equal($"{operationPart}.User.{ErrorCodeReasons.Required}", code.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("User.Name")]
    public void For_RejectsInvalidEntityOrReason(string part)
    {
        Assert.Throws<ArgumentException>(() => new ErrorContext(OperationType.Create, part));
        Assert.Throws<ArgumentException>(() => ErrorCode.For(new ErrorContext(OperationType.Create, "User"), part));
    }

    [Fact]
    public void For_RejectsNullContext()
    {
        Assert.Throws<ArgumentNullException>(() => ErrorCode.For(null!, "Reason"));
    }

    [Fact]
    public void For_RejectsUndefinedOperation()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ErrorCode.For(new ErrorContext((OperationType)999, "User"), "Reason"));
        Assert.Equal("operation", ex.ParamName);
    }

    [Fact]
    public void CodesHaveValueSemantics()
    {
        Assert.Equal(ErrorCode.From("same"), ErrorCode.From("same"));
        Assert.NotEqual(ErrorCode.From("same"), ErrorCode.From("other"));
    }

    [Fact]
    public void BuiltInFactoryUsesReasonConstant()
    {
        var error = Megaraz.ResultPattern.Error.NotFound(new ErrorContext(OperationType.Get, "User"));
        Assert.Equal($"Get.User.{ErrorCodeReasons.NotFound}", error.Code);
    }
}
