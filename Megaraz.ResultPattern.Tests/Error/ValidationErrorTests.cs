namespace Megaraz.ResultPattern.Tests.Error;

public class ValidationErrorTests
{
    [Fact]
    public void CustomFactoriesPreserveDescriptionAndResolveMessagesAndFields()
    {
        var context = TestData.FieldContext;
        var custom = ValidationError.Custom(context, "description");
        Assert.Equal("Update.User.Custom", custom.Code);
        Assert.Equal(ValidationErrorType.Custom, custom.ValidationErrorType);
        Assert.Equal("description", custom.Description);
        Assert.Equal("description", custom.UserMessage);
        Assert.Equal("Email", custom.FieldName);

        var owned = ValidationError.Custom(context, ErrorCode.From("owned"), "description", "friendly", "Name");
        Assert.Equal("owned", owned.Code);
        Assert.Equal("friendly", owned.UserMessage);
        Assert.Equal("Name", owned.FieldName);

        var reason = ValidationError.CustomWithReason(context, "Specific", "details");
        Assert.Equal("Update.User.Specific", reason.Code);
    }

    [Theory]
    [InlineData(null, "Email")]
    [InlineData("Name", "Name")]
    public void ExplicitFieldOverridesContext(string? explicitField, string expected)
    {
        var error = ValidationError.Custom(TestData.FieldContext, "details", fieldName: explicitField);
        Assert.Equal(expected, error.FieldName);
    }

    [Theory]
    [InlineData("Required", ValidationErrorType.Required)]
    [InlineData("InvalidFormat", ValidationErrorType.InvalidFormat)]
    [InlineData("OutOfRange", ValidationErrorType.OutOfRange)]
    [InlineData("TooShort", ValidationErrorType.TooShort)]
    [InlineData("TooLong", ValidationErrorType.TooLong)]
    [InlineData("AlreadyExists", ValidationErrorType.AlreadyExists)]
    public void BuiltInFactoriesExposeExpectedTypeCodeAndCustomMessage(string reason, ValidationErrorType type)
    {
        var context = TestData.FieldContext;
        ValidationError error = reason switch
        {
            "Required" => ValidationError.Required(context, "friendly"),
            "InvalidFormat" => ValidationError.InvalidFormat(context, "email", "friendly"),
            "OutOfRange" => ValidationError.OutOfRange(context, "1-5", "friendly"),
            "TooShort" => ValidationError.TooShort(context, "3", "friendly"),
            "TooLong" => ValidationError.TooLong(context, "10", "friendly"),
            _ => ValidationError.AlreadyExists(context, "friendly")
        };
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(type, error.ValidationErrorType);
        Assert.Equal($"Update.User.{reason}", error.Code);
        Assert.Equal("friendly", error.UserMessage);
        Assert.Equal("Email", error.FieldName);
        Assert.NotEmpty(error.Description);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BuiltInDescriptionsSupportFieldPresence(bool fieldPresent)
    {
        var context = new ErrorContext(OperationType.Create, "User", fieldPresent ? "Email" : null);
        Assert.Contains(fieldPresent ? "field 'Email'" : "provided value", ValidationError.InvalidFormat(context, "email").Description);
        Assert.Contains(fieldPresent ? "field 'Email'" : "provided value", ValidationError.OutOfRange(context, "1-5").Description);
        Assert.Contains(fieldPresent ? "field 'Email'" : "provided value", ValidationError.TooShort(context, "3").Description);
        Assert.Contains(fieldPresent ? "field 'Email'" : "provided value", ValidationError.TooLong(context, "10").Description);
        Assert.Contains(fieldPresent ? "with that Email" : "User already", ValidationError.AlreadyExists(context).Description);
        Assert.Contains(fieldPresent ? "'Email'" : "'User'", ValidationError.Required(context).Description);
    }

    [Fact]
    public void BuiltInValidationFactoriesAcceptLocalizedDescriptions()
    {
        var error = ValidationError.InvalidFormat(
            TestData.FieldContext,
            "email",
            userMessage: "friendly",
            messageFactory: context => $"localized {context.FieldName}");

        Assert.Equal("localized Email", error.Description);
        Assert.Equal("friendly", error.UserMessage);
    }

    [Fact]
    public void NonMatchingValuesSupportsLabelsAndCustomMessage()
    {
        var both = ValidationError.NonMatchingValues(TestData.FieldContext, "ConfirmEmail", "friendly");
        Assert.Equal("Update.User.NonMatchingValues", both.Code);
        Assert.Equal(ValidationErrorType.NonMatchingValues, both.ValidationErrorType);
        Assert.Equal("The values for 'Email' and 'ConfirmEmail' do not match.", both.Description);
        Assert.Equal("friendly", both.UserMessage);
        Assert.Equal("ConfirmEmail", both.FieldName);

        var missing = ValidationError.NonMatchingValues(TestData.FieldContext);
        Assert.Equal("The provided values do not match.", missing.Description);
        Assert.Equal("Email", missing.FieldName);
    }

    [Fact]
    public void FactoriesRejectInvalidArguments()
    {
        Assert.Throws<ArgumentNullException>(() => ValidationError.Custom(null!, "x"));
        Assert.Throws<ArgumentNullException>(() => ValidationError.Custom(TestData.Context, (ErrorCode)null!, "x"));
        Assert.Throws<ArgumentNullException>(() => ValidationError.CustomWithReason(null!, "x", "y"));
        Assert.Throws<ArgumentException>(() => ValidationError.CustomWithReason(TestData.Context, "bad.reason", "y"));
        Assert.Throws<ArgumentNullException>(() => ValidationError.Required(null!));
        Assert.Throws<ArgumentNullException>(() => ValidationError.InvalidFormat(null!, "x"));
        Assert.Throws<ArgumentNullException>(() => ValidationError.OutOfRange(null!, "x"));
        Assert.Throws<ArgumentNullException>(() => ValidationError.TooShort(null!, "x"));
        Assert.Throws<ArgumentNullException>(() => ValidationError.TooLong(null!, "x"));
        Assert.Throws<ArgumentNullException>(() => ValidationError.AlreadyExists(null!));
        Assert.Throws<ArgumentNullException>(() => ValidationError.NonMatchingValues(null!));
    }

    [Fact]
    public void ValidationErrorsHaveValueSemantics()
    {
        Assert.Equal(ValidationError.Required(TestData.FieldContext), ValidationError.Required(TestData.FieldContext));
        Assert.NotEqual(ValidationError.Required(TestData.FieldContext),
            ValidationError.Custom(TestData.FieldContext, "different"));
    }
}
