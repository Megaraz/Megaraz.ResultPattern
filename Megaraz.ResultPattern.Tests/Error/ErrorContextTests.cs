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
}
