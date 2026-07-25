namespace Megaraz.ResultPattern.Tests;

internal static class TestData
{
    public static readonly ErrorContext Context = new(OperationType.Create, "User");
    public static readonly ErrorContext FieldContext = new(OperationType.Update, "User", "Email");
    public static readonly ValidationError ValidationError = ValidationError.Required(FieldContext);
    public static readonly Megaraz.ResultPattern.Error Error =
        Megaraz.ResultPattern.Error.Custom("User.Failure", "A failure occurred.", ErrorType.Failure);
}
