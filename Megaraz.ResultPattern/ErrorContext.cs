namespace Megaraz.ResultPattern;

/// <summary>
/// Describes the operation and application context associated with an error.
/// </summary>
/// <param name="Operation">The kind of operation being performed.</param>
/// <param name="EntityName">The logical entity or resource involved.</param>
/// <param name="FieldName">The property or field involved, when applicable.</param>
public record ErrorContext(
    OperationType Operation,
    string EntityName,
    string? FieldName = null);

/// <summary>
/// Identifies the kind of operation associated with an error.
/// </summary>
public enum OperationType
{
    /// <summary>A caller-defined operation.</summary>
    Custom = 0,
    /// <summary>A create operation.</summary>
    Create = 1,
    /// <summary>A single-resource retrieval operation.</summary>
    Get = 2,
    /// <summary>A collection retrieval operation.</summary>
    GetCollection = 3,
    /// <summary>An update operation.</summary>
    Update = 4,
    /// <summary>A delete operation.</summary>
    Delete = 5,
    /// <summary>An authentication operation.</summary>
    Login = 100
}
