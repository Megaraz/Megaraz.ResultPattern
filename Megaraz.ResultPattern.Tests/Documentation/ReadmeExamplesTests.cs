namespace Megaraz.ResultPattern.Tests.Documentation;

public class ReadmeExamplesTests
{
    [Fact]
    public void ValidationExampleCreatesValidationFailureContainingTheError()
    {
        string? email = null;
        var context = new ErrorContext(OperationType.Create, "User", "Email");

        var error = email.ValidateRequired(context, "Email");
        var result = error is not null
            ? Result.ValidationFailure([error])
            : Result.Success();

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationError>(result.PrimaryError);
        Assert.Same(result.PrimaryError, Assert.Single(result.ValidationErrors));
    }
}
