namespace Megaraz.ResultPattern.Tests.Error;

using ResultError = Megaraz.ResultPattern.Error;

public class ErrorTests
{
    [Fact]
    public void CustomPreservesAllSuppliedValues()
    {
        var exception = new InvalidOperationException("boom");
        var error = ResultError.Custom("custom", "technical", ErrorType.Conflict, "friendly", exception);
        Assert.Equal("custom", error.Code);
        Assert.Equal("technical", error.Description);
        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("friendly", error.UserMessage);
        Assert.Same(exception, error.Exception);
    }

    [Fact]
    public void CustomNormalizesNullUserMessageAndAcceptsCallerOwnedCode()
    {
        var error = ResultError.Custom(ErrorCode.From("account/v2"), "technical", ErrorType.Failure, null);
        Assert.Equal("account/v2", error.Code);
        Assert.Equal(string.Empty, error.UserMessage);
    }

    [Fact]
    public void CustomWithContextCreatesConventionalCode()
    {
        var error = ResultError.Custom(TestData.Context, "Broken", ErrorType.Failure, "technical");
        Assert.Equal("Create.User.Broken", error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CustomRejectsBlankCode(string? code)
    {
        var ex = Assert.Throws<ArgumentException>(() => ResultError.Custom(code!, "description", ErrorType.Failure));
        Assert.Equal("code", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void CustomRejectsBlankDescription(string? description)
    {
        var ex = Assert.Throws<ArgumentException>(() => ResultError.Custom("code", description!, ErrorType.Failure));
        Assert.Equal("description", ex.ParamName);
    }

    [Theory]
    [InlineData(ErrorType.None)]
    [InlineData(ErrorType.Validation)]
    public void CustomRejectsReservedTypes(ErrorType type)
    {
        var ex = Assert.Throws<ArgumentException>(() => ResultError.Custom("code", "description", type));
        Assert.Equal("type", ex.ParamName);
    }

    [Fact]
    public void CustomRejectsUndefinedType()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => ResultError.Custom("code", "description", (ErrorType)999));
        Assert.Equal("type", ex.ParamName);
    }

    [Fact]
    public void CustomRejectsNullCodeObjectAndContext()
    {
        Assert.Throws<ArgumentNullException>(() => ResultError.Custom((ErrorCode)null!, "description", ErrorType.Failure));
        Assert.Throws<ArgumentNullException>(() => ResultError.Custom((ErrorContext)null!, "Reason", ErrorType.Failure, "description"));
    }

    [Fact]
    public void ConvenienceFactoriesProduceDocumentedValues()
    {
        var context = new ErrorContext(OperationType.Get, "User");
        var cases = new[]
        {
            (ResultError.NotFound(context), ErrorType.NotFound, ErrorCodeReasons.NotFound, "User not found"),
            (ResultError.Conflict(context), ErrorType.Conflict, ErrorCodeReasons.Conflict, "Unique User constraint violated."),
            (ResultError.Forbidden(context), ErrorType.Forbidden, ErrorCodeReasons.Forbidden, "Forbidden access."),
            (ResultError.Cancelled(context), ErrorType.Cancelled, ErrorCodeReasons.Cancelled, "The operation on User was cancelled.")
        };
        foreach (var (error, type, reason, message) in cases)
        {
            Assert.Equal(type, error.Type);
            Assert.Equal($"Get.User.{reason}", error.Code);
            Assert.Equal(message, error.Description);
            Assert.Equal(message, error.UserMessage);
        }
    }

    [Fact]
    public void UnauthorizedUsesFieldAwareAndFieldFreeWording()
    {
        Assert.Equal("Unauthorized access to Email", ResultError.Unauthorized(TestData.FieldContext).Description);
        Assert.Equal("Unauthorized access", ResultError.Unauthorized(TestData.Context).Description);
    }

    [Fact]
    public void FailureUsesDefaultAndExplicitValues()
    {
        var defaultError = ResultError.Failure(TestData.Context);
        Assert.Equal("An unexpected failure occurred while processing User.", defaultError.Description);
        Assert.Equal(defaultError.Description, defaultError.UserMessage);

        var exception = new Exception("x");
        var explicitError = ResultError.Failure(TestData.Context, "technical", exception, "friendly");
        Assert.Equal("technical", explicitError.Description);
        Assert.Equal("friendly", explicitError.UserMessage);
        Assert.Same(exception, explicitError.Exception);
    }

    [Fact]
    public void ConvenienceFactoriesRejectNullContext()
    {
        Assert.Throws<ArgumentNullException>(() => ResultError.NotFound(null!));
        Assert.Throws<ArgumentNullException>(() => ResultError.Conflict(null!));
        Assert.Throws<ArgumentNullException>(() => ResultError.Unauthorized(null!));
        Assert.Throws<ArgumentNullException>(() => ResultError.Forbidden(null!));
        Assert.Throws<ArgumentNullException>(() => ResultError.Failure(null!));
        Assert.Throws<ArgumentNullException>(() => ResultError.Cancelled(null!));
    }

    [Fact]
    public void NoneIsSharedEmptySentinelAndRecordsHaveValueSemantics()
    {
        Assert.Same(ResultError.None, ResultError.None);
        Assert.Equal(string.Empty, ResultError.None.Code);
        Assert.Equal(string.Empty, ResultError.None.Description);
        Assert.Equal(ErrorType.None, ResultError.None.Type);
        Assert.Equal(string.Empty, ResultError.None.UserMessage);
        Assert.Null(ResultError.None.Exception);
        Assert.Equal(ResultError.Custom("x", "y", ErrorType.Failure), ResultError.Custom("x", "y", ErrorType.Failure));
        Assert.NotEqual(ResultError.Custom("x", "y", ErrorType.Failure), ResultError.Custom("x", "z", ErrorType.Failure));
    }

    [Fact]
    public void ToStringUsesCodeAndDescriptionLayout()
    {
        var text = ResultError.Custom("A.Code", "Details", ErrorType.Failure).ToString();
        Assert.Equal($"Error Code: A.Code{Environment.NewLine}Description: Details", text);
    }

    private sealed record InvalidValidationError : Megaraz.ResultPattern.Error
    {
        public InvalidValidationError() : base("bad", "bad", ErrorType.Validation) { }
    }

    [Fact]
    public void NonValidationSubclassRejectsValidationType() => Assert.Throws<ArgumentException>(() => new InvalidValidationError());
}
