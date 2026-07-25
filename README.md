# Megaraz.ResultPattern

`Megaraz.ResultPattern` is a focused .NET library for representing successful operations,
failures, and validation errors without coupling consumers to web or persistence frameworks.

```csharp
var result = Result<string>.Success("value");

if (result.IsSuccess)
{
    Console.WriteLine(result.Value);
}
```

Successful `Result<TValue>` instances require a non-null value. Failed results expose a
`PrimaryError`, while validation failures also expose their `ValidationErrors`.

## Error codes

Built-in factories retain conventional codes such as `Create.User.NotFound`. For
application-owned contracts, provide any stable code directly; the library does not
require an `Operation.Entity.Reason` format:

```csharp
var error = Error.Custom(
    ErrorCode.From("accounts.email-already-registered"),
    "The email address is already registered.",
    ErrorType.Conflict);

var validationError = ValidationError.CustomWithReason(
    new ErrorContext(OperationType.Create, "User", "Email"),
    "EmailAlreadyRegistered",
    "The email address is already registered.");
```

`CustomWithReason` is an opt-in convenience that produces
`Create.User.EmailAlreadyRegistered`; use `ErrorCode.From` when the code shape is
owned by your application.

## Extension errors

`ErrorType.External` is intended for errors supplied by extension packages or
external dependencies. Extension packages can derive from `Error` and expose
their own detailed error enum while using `External` as the shared top-level
classification:

```csharp
var error = Error.Custom(
    ErrorCode.From("payments.gateway-timeout"),
    "The payment gateway did not respond in time.",
    ErrorType.External);
```

`ErrorContext` requires a defined operation and a non-blank logical entity or
resource name. Its optional `FieldName` is metadata for validation and
presentation; it is not part of conventional error codes and may use labels
such as `Profile.Email`.

Built-in error factories provide technical fallback descriptions, but do not
assume a presentation language: `UserMessage` is empty unless supplied. Use the
optional `messageFactory` argument to create localized or application-specific
descriptions in an extension or application layer.

For validation, the `Validate...` methods return a `ValidationError` when a
check fails and `null` when it passes:

```csharp
var error = email.ValidateRequired(context, "Email");
if (error is not null)
    return Result.Failure(error);
```

The existing boolean validation methods remain available for compatibility and
return `true` when the invalid condition is detected.

Install from NuGet:

```bash
dotnet add package Megaraz.ResultPattern
```