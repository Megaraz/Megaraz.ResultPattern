namespace Megaraz.ResultPattern.Tests.Results;

public class ResultTests
{
    [Fact]
    public void SuccessHasEmptyState()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Message);
        Assert.Empty(result.ValidationErrors);
        Assert.Same(Megaraz.ResultPattern.Error.None, result.PrimaryError);
    }

    [Fact]
    public void FailureUsesErrorMessageAndRejectsValidationErrors()
    {
        var error = TestData.Error;
        var result = Result.Failure(error);
        Assert.True(result.IsFailure);
        Assert.Equal(error.UserMessage, result.Message);
        Assert.Same(error, result.PrimaryError);
        Assert.Empty(result.ValidationErrors);

        var explicitResult = Result.Failure(error, "explicit");
        Assert.Equal("explicit", explicitResult.Message);
        Assert.Throws<ArgumentException>(() => Result.Failure(TestData.ValidationError));
    }

    [Fact]
    public void FailureRejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Failure(null!));
        Assert.Throws<ArgumentNullException>(() => Result.Failure(TestData.Error, null!));
    }

    [Fact]
    public void ValidationFailurePreservesOrderAndUsesFirstError()
    {
        var first = ValidationError.Required(TestData.FieldContext);
        var second = ValidationError.InvalidFormat(TestData.FieldContext, "email");
        var result = Result.ValidationFailure(new[] { first, second }, "invalid input");
        Assert.Equal("invalid input", result.Message);
        Assert.Same(first, result.PrimaryError);
        Assert.Equal(new[] { first, second }, result.ValidationErrors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidationFailureUsesDefaultForBlankMessage(string? message)
    {
        var result = Result.ValidationFailure(new[] { TestData.ValidationError }, message);
        Assert.Empty(result.Message);
    }

    [Fact]
    public void ValidationFailureRejectsInvalidCollectionsAndCopiesSource()
    {
        Assert.Throws<ArgumentNullException>(() => Result.ValidationFailure(null!));
        Assert.Throws<ArgumentException>(() => Result.ValidationFailure(Array.Empty<ValidationError>()));
        Assert.Throws<ArgumentException>(() => Result.ValidationFailure(new ValidationError[] { TestData.ValidationError, null! }));
        var source = new[] { TestData.ValidationError };
        var result = Result.ValidationFailure(source);
        source[0] = ValidationError.Required(TestData.Context);
        Assert.Same(TestData.ValidationError, result.ValidationErrors.Single());
    }
}
