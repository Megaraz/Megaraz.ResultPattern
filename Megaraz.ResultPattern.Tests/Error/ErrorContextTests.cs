namespace Megaraz.ResultPattern.Tests.Error;

public class ErrorContextTests
{
    [Fact]
    public void EqualityIncludesOperationEntityAndField()
    {
        Assert.Equal(new ErrorContext(OperationType.Get, "User", "Id"),
            new ErrorContext(OperationType.Get, "User", "Id"));
        Assert.NotEqual(new ErrorContext(OperationType.Get, "User", "Id"),
            new ErrorContext(OperationType.Get, "User", "Name"));
        Assert.NotEqual(new ErrorContext(OperationType.Get, "User", "Id"),
            new ErrorContext(OperationType.Update, "User", "Id"));
    }

    [Fact]
    public void WithCreatesChangedCopyAndSupportsNullField()
    {
        var context = new ErrorContext(OperationType.Get, "User", "Id");
        var changed = context with { FieldName = null };
        Assert.Equal(new ErrorContext(OperationType.Get, "User"), changed);
        Assert.Null(changed.FieldName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorRejectsMissingEntityName(string? entityName)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new ErrorContext(OperationType.Get, entityName!));

        Assert.Equal("entityName", ex.ParamName);
    }

    [Fact]
    public void ConstructorRejectsEntityNamesThatBreakConventionalCodes()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new ErrorContext(OperationType.Get, "User.Profile"));

        Assert.Equal("entityName", ex.ParamName);
    }

    [Fact]
    public void ConstructorRejectsUndefinedOperations()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ErrorContext((OperationType)999, "User"));

        Assert.Equal("operation", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ConstructorRejectsBlankFieldNames(string fieldName)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new ErrorContext(OperationType.Get, "User", fieldName));

        Assert.Equal("fieldName", ex.ParamName);
    }

    [Fact]
    public void FieldNameIsOptionalMetadataAndMayContainDots()
    {
        var context = new ErrorContext(OperationType.Update, "User", "Profile.Email");

        Assert.Equal("Profile.Email", context.FieldName);
        Assert.Equal("Update.User.Required", ErrorCode.For(context, ErrorCodeReasons.Required).Code);
    }

    [Fact]
    public void WithRejectsInvalidOperation()
    {
        var context = new ErrorContext(OperationType.Get, "User");

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => context with { Operation = (OperationType)999 });

        Assert.Equal("operation", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("User.Profile")]
    public void WithRejectsInvalidEntityName(string entityName)
    {
        var context = new ErrorContext(OperationType.Get, "User");

        var ex = Assert.Throws<ArgumentException>(() => context with { EntityName = entityName });

        Assert.Equal("entityName", ex.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void WithRejectsBlankFieldName(string fieldName)
    {
        var context = new ErrorContext(OperationType.Get, "User");

        var ex = Assert.Throws<ArgumentException>(() => context with { FieldName = fieldName });

        Assert.Equal("fieldName", ex.ParamName);
    }
}
