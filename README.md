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

Install from NuGet:

```bash
dotnet add package Megaraz.ResultPattern
```