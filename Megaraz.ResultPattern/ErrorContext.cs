namespace Megaraz.ResultPattern;

/// <summary>
/// Describes the operation and application context associated with an error.
/// </summary>
/// <param name="Layer">The application layer where the error was created.</param>
/// <param name="ServiceName">The component or service where the error was created.</param>
/// <param name="MethodName">The method where the error was created.</param>
/// <param name="Operation">The kind of operation being performed.</param>
/// <param name="EntityName">The logical entity or resource involved.</param>
/// <param name="FieldName">The property or field involved, when applicable.</param>
public record ErrorContext(
    string Layer,
    string ServiceName,
    string MethodName,
    OperationType Operation,
    string EntityName,
    string? FieldName = null);

/// <summary>
/// Identifies the kind of operation associated with an error.
/// </summary>
public enum OperationType
{
    Custom = 0,
    Create = 1,
    Get = 2,
    GetCollection = 3,
    Update = 4,
    Delete = 5,
    Login = 100
}
