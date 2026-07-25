namespace Megaraz.ResultPattern.Tests.Results;

public class ResultOfTTests
{
    [Fact]
    public void SuccessReturnsReferenceAndValueTypes()
    {
        Assert.Equal("value", Result<string>.Success("value").Value);
        Assert.Equal(42, Result<int>.Success(42).Value);
        Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));
    }

    [Fact]
    public void SuccessHasExpectedBaseState()
    {
        var result = Result<string>.Success("value");
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Message);
        Assert.Empty(result.ValidationErrors);
        Assert.Same(ResultPattern.Error.None, result.PrimaryError);
    }

    [Fact]
    public void NormalFailurePreservesMessageAndPrimaryError()
    {
        var result = Result<string>.Failure(TestData.Error);
        Assert.True(result.IsFailure);
        Assert.Equal(TestData.Error.UserMessage, result.Message);
        Assert.Same(TestData.Error, result.PrimaryError);
        Assert.Empty(result.ValidationErrors);
        Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Equal("explicit", Result<int>.Failure(TestData.Error, "explicit").Message);
    }

    [Fact]
    public void FailureRejectsInvalidInputs()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.Failure(null!));
        Assert.Throws<ArgumentNullException>(() => Result<string>.Failure(TestData.Error, null!));
        Assert.Throws<ArgumentException>(() => Result<string>.Failure(TestData.ValidationError));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidationFailureUsesDefaultMessage(string? message)
    {
        var result = Result<string>.ValidationFailure(new[] { TestData.ValidationError }, message);
        Assert.True(result.IsFailure);
        Assert.Equal("Validation errors occurred, see validation errors for details.", result.Message);
        Assert.Same(TestData.ValidationError, result.PrimaryError);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ValidationFailureRejectsInvalidCollectionsAndCopiesSource()
    {
        Assert.Throws<ArgumentNullException>(() => Result<string>.ValidationFailure(null!));
        Assert.Throws<ArgumentException>(() => Result<string>.ValidationFailure(Array.Empty<ValidationError>()));
        Assert.Throws<ArgumentException>(() => Result<string>.ValidationFailure(new ValidationError[] { null! }));
        var source = new[] { TestData.ValidationError };
        var result = Result<string>.ValidationFailure(source);
        source[0] = ValidationError.Required(TestData.Context);
        Assert.Same(TestData.ValidationError, result.ValidationErrors.Single());
    }
}
