namespace Megaraz.ResultPattern.Tests.Results;

public class ResultExtensionsTests
{
    [Fact]
    public void MapInvokesMapperOnceForSuccess()
    {
        var calls = 0;
        var mapped = Result<int>.Success(3).Map(value => { calls++; return value.ToString(); });
        Assert.Equal(1, calls);
        Assert.True(mapped.IsSuccess);
        Assert.Equal("3", mapped.Value);
    }

    [Fact]
    public void MapPreservesNormalFailureWithoutInvokingMapper()
    {
        var error = TestData.Error;
        var source = Result<int>.Failure(error, "message");
        var calls = 0;
        var mapped = source.Map(_ => { calls++; return "value"; });
        Assert.Equal(0, calls);
        Assert.Equal("message", mapped.Message);
        Assert.Same(error, mapped.PrimaryError);
        Assert.Empty(mapped.ValidationErrors);
    }

    [Fact]
    public void MapPreservesValidationFailure()
    {
        var first = TestData.ValidationError;
        var second = ValidationError.InvalidFormat(TestData.FieldContext, "email");
        var mapped = Result<int>.ValidationFailure(new[] { first, second }).Map(_ => "value");
        Assert.Same(first, mapped.PrimaryError);
        Assert.Equal(new[] { first, second }, mapped.ValidationErrors);
    }

    [Fact]
    public void MapRejectsNullInputsPropagatesMapperErrorsAndRejectsNullOutput()
    {
        Result<int>? nullResult = null;
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.Map<int, int>(nullResult!, _ => 1));
        Assert.Throws<ArgumentNullException>(() => ResultExtensions.Map<int, string>(Result<int>.Success(1), null!));
        Assert.Throws<ArgumentNullException>(() => Result<int>.Success(1).Map(_ => (string)null!));
        var exception = new InvalidOperationException();
        Assert.Same(exception, Assert.Throws<InvalidOperationException>(() =>
            Result<int>.Success(1).Map<int, int>(_ => throw exception)));
    }

    [Fact]
    public async Task MapAsyncPreservesSuccessAndFailure()
    {
        var success = await Result<int>.Success(3)
            .MapAsync(value => Task.FromResult(value.ToString()));
        Assert.Equal("3", success.Value);

        var failure = Result<string>.Failure(TestData.Error, "message");
        var mappedFailure = await failure.MapAsync(value => Task.FromResult(value.Length));
        Assert.Equal("message", mappedFailure.Message);
        Assert.Same(TestData.Error, mappedFailure.PrimaryError);
    }

    [Fact]
    public void ResultConversionsAllowTypedUpcastAndFailureDowncastOnly()
    {
        Result untyped = Result<int>.Success(3);
        Assert.True(untyped.IsSuccess);

        var typed = Result<int>.FromResult(Result.Failure(TestData.Error, "message"));
        Assert.Equal("message", typed.Message);
        Assert.Throws<ArgumentException>(() => Result<int>.FromResult(Result.Success()));
    }

}
