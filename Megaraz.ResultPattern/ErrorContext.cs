namespace Megaraz.ResultPattern;

/// <summary>
/// Describes the operation and application context associated with an error.
/// </summary>
public record ErrorContext
{
    private OperationType _operation;
    private string _entityName = null!;
    private string? _fieldName;

    /// <summary>
    /// Initializes an error context.
    /// </summary>
    /// <param name="operation">The kind of operation being performed.</param>
    /// <param name="entityName">The logical entity or resource involved. It is also used as a segment in conventional error codes.</param>
    /// <param name="fieldName">The optional property or field label involved. It is metadata and is not included in conventional error codes.</param>
    public ErrorContext(OperationType operation, string entityName, string? fieldName = null)
    {
        ValidateOperation(operation);
        ValidateEntityName(entityName);
        ValidateFieldName(fieldName);

        Operation = operation;
        EntityName = entityName;
        FieldName = fieldName;
    }

    /// <summary>Gets the kind of operation being performed.</summary>
    public OperationType Operation
    {
        get => _operation;
        init
        {
            ValidateOperation(value);
            _operation = value;
        }
    }

    /// <summary>Gets the logical entity or resource involved.</summary>
    public string EntityName
    {
        get => _entityName;
        init
        {
            ValidateEntityName(value);
            _entityName = value;
        }
    }

    /// <summary>Gets the optional property or field label involved.</summary>
    public string? FieldName
    {
        get => _fieldName;
        init
        {
            ValidateFieldName(value);
            _fieldName = value;
        }
    }

    internal void Validate()
    {
        ValidateOperation(Operation);
        ValidateEntityName(EntityName);
        ValidateFieldName(FieldName);
    }

    private static void ValidateOperation(OperationType operation)
    {
        if (!Enum.IsDefined(operation))
            throw new ArgumentOutOfRangeException(nameof(operation), operation, "The operation must be a defined value.");
    }

    private static void ValidateEntityName(string? entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Entity name cannot be null, empty, or whitespace.", nameof(entityName));
        if (entityName.Contains('.'))
            throw new ArgumentException("Entity name cannot contain '.'.", nameof(entityName));
    }

    private static void ValidateFieldName(string? fieldName)
    {
        if (fieldName is not null && string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentException("Field name cannot be empty or whitespace when specified.", nameof(fieldName));
    }
}

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
