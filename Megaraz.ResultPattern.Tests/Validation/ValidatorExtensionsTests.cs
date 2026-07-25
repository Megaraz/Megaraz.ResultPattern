namespace Megaraz.ResultPattern.Tests.Validation;

public class ValidatorExtensionsTests
{
    [Fact]
    public void RequiredFieldsValidAndEmptyReturnFalse()
    {
        var values = new (string FieldName, string? Value)[] { ("First", "one"), ("Second", "two") };
        Assert.False(values.RequiredFieldsAreNullOrWhiteSpace(TestData.Context, out var errors));
        Assert.Empty(errors);
        Assert.False(Array.Empty<(string, string?)>().RequiredFieldsAreNullOrWhiteSpace(TestData.Context, out errors));
        Assert.Empty(errors);
    }

    [Fact]
    public void RequiredFieldsReturnsOrderedErrorPerFailingField()
    {
        var values = new[] { ("First", null), ("Second", " "), ("Third", "ok") };
        Assert.True(values.RequiredFieldsAreNullOrWhiteSpace(TestData.Context, out var errors));
        Assert.Equal(2, errors.Count);
        Assert.Equal(new[] { "First", "Second" }, errors.Select(error => error.FieldName));
    }

    [Fact]
    public void RequiredFieldsRejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IEnumerable<(string FieldName, string? Value)>)null!).RequiredFieldsAreNullOrWhiteSpace(TestData.Context, out _));
        Assert.Throws<ArgumentNullException>(() =>
            Array.Empty<(string, string?)>().RequiredFieldsAreNullOrWhiteSpace(null!, out _));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("value", false)]
    public void IsNullOrWhiteSpaceWithFieldResolvesField(string? value, bool invalid)
    {
        var context = new ErrorContext(OperationType.Create, "User", "ContextField");
        var result = value.IsNullOrWhiteSpace("Explicit", context, out var error);
        Assert.Equal(invalid, result);
        if (invalid) Assert.Equal("Explicit", error.FieldName);
        else Assert.Null(error);
    }

    [Fact]
    public void IsNullOrWhiteSpaceFieldFallbackUsesContextThenValue()
    {
        "".IsNullOrWhiteSpace(" ", TestData.FieldContext, out var contextError);
        Assert.Equal("Email", contextError.FieldName);
        "".IsNullOrWhiteSpace(" ", TestData.Context, out var fallbackError);
        Assert.Null(fallbackError.FieldName);
    }

    [Fact]
    public void IsNullOrWhiteSpaceWithContextUsesContextField()
    {
        Assert.True(((string?)null).IsNullOrWhiteSpace(TestData.FieldContext, out var error));
        Assert.Equal("Email", error.FieldName);
        Assert.False("ok".IsNullOrWhiteSpace(TestData.FieldContext, out error));
        Assert.Null(error);
        Assert.Throws<ArgumentNullException>(() => "x".IsNullOrWhiteSpace("Field", null!, out _));
        Assert.Throws<ArgumentNullException>(() => "x".IsNullOrWhiteSpace(null!, out _));
    }

    [Fact]
    public void DoesNotMatchValidatesInOrderAndComparesOrdinal()
    {
        Assert.True(((string?)null).DoesNotMatch("x", "First", "Second", TestData.Context, out var first));
        Assert.Equal("First", first.FieldName);
        Assert.True("x".DoesNotMatch(null, "First", "Second", TestData.Context, out var second));
        Assert.Equal("Second", second.FieldName);
        Assert.True("a".DoesNotMatch("A", "First", "Second", TestData.Context, out var mismatch));
        Assert.Equal(ValidationErrorType.NonMatchingValues, mismatch.ValidationErrorType);
        Assert.False("a".DoesNotMatch("a", "First", "Second", TestData.Context, out var none));
        Assert.Null(none);
        Assert.Throws<ArgumentNullException>(() => "a".DoesNotMatch("a", "First", "Second", null!, out _));
    }

    [Fact]
    public void ValidateMethodsReturnNullWhenValuesAreValid()
    {
        Assert.Null("ok".ValidateRequired(TestData.Context));
        Assert.Empty(new (string FieldName, string? Value)[] { ("First", "ok") }
            .ValidateRequiredFields(TestData.Context));
        Assert.Null("a".ValidateDoesNotMatch("a", "First", "Second", TestData.Context));
    }

    [Fact]
    public void ValidateMethodsReturnClearErrorsWhenValuesAreInvalid()
    {
        var required = ((string?)null).ValidateRequired(TestData.Context);
        Assert.NotNull(required);
        Assert.Null(required!.FieldName);

        var fields = new[] { ("First", (string?)null), ("Second", " ") }
            .ValidateRequiredFields(TestData.Context);
        Assert.Equal(new[] { "First", "Second" }, fields.Select(error => error.FieldName));

        var mismatch = "a".ValidateDoesNotMatch("b", null, null, TestData.Context);
        Assert.NotNull(mismatch);
        Assert.Null(mismatch!.FieldName);
    }

    [Fact]
    public void DoesNotMatchUsesBothLabelsAndFallbacks()
    {
        Assert.True("a".DoesNotMatch("b", "First", "Second", TestData.Context, out var labeled));
        Assert.Contains("'First' and 'Second'", labeled.Description);
        Assert.True("a".DoesNotMatch("b", "", "", TestData.FieldContext, out var fallback));
        Assert.Equal("Email", fallback.FieldName);
    }
}
