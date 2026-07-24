namespace Megaraz.ResultPattern
{
    /// <summary>
    /// Validation extension methods that produce <see cref="ValidationError"/> instances.
    /// </summary>
    /// <remarks>
    /// Convention: all methods return <see langword="true"/> when the validation check <b>fails</b>
    /// (i.e., the invalid condition is detected). This matches the BCL pattern used by
    /// <see cref="string.IsNullOrWhiteSpace"/> and makes validation code read naturally without double-negatives:
    /// <code>if (value.IsNullOrWhiteSpace(ctx, out var e)) errors.Add(e);</code>
    /// </remarks>
    public static class ValidatorExtensions
    {

        /// <summary>
        /// Returns <see langword="true"/> if any field in <paramref name="requiredValues"/> is null or whitespace
        /// and populates <paramref name="validationErrors"/> with one error per failing field.
        /// Returns <see langword="false"/> if all fields have content.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="requiredValues"/> or <paramref name="errorContext"/> is <see langword="null"/>.</exception>
        public static bool RequiredFieldsAreNullOrWhiteSpace(
            this IEnumerable<(string FieldName, string? Value)> requiredValues,
            ErrorContext errorContext,
            out IReadOnlyList<ValidationError> validationErrors)
        {
            ArgumentNullException.ThrowIfNull(requiredValues);
            ArgumentNullException.ThrowIfNull(errorContext);

            var errors = new List<ValidationError>();

            foreach (var (fieldName, value) in requiredValues)
            {
                if (value.IsNullOrWhiteSpace(fieldName, errorContext, out var error))
                    errors.Add(error);
            }

            validationErrors = errors;
            return errors.Count > 0;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is null or whitespace
        /// and populates <paramref name="nullOrEmptyError"/>. Returns <see langword="false"/> otherwise.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errorContext"/> is <see langword="null"/>.</exception>
        public static bool IsNullOrWhiteSpace(this string? value, string fieldName, ErrorContext errorContext, out ValidationError nullOrEmptyError)
        {
            ArgumentNullException.ThrowIfNull(errorContext);

            nullOrEmptyError = default!;

            if (string.IsNullOrWhiteSpace(value))
            {
                string resolvedFieldName = string.IsNullOrWhiteSpace(fieldName) ? errorContext.FieldName ?? nameof(value) : fieldName;

                nullOrEmptyError = ValidationError.Required(errorContext with { FieldName = resolvedFieldName });
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value"/> is null or whitespace
        /// and populates <paramref name="nullOrEmptyError"/>. Returns <see langword="false"/> otherwise.
        /// Uses <see cref="ErrorContext.FieldName"/> as the field label in the error description.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errorContext"/> is <see langword="null"/>.</exception>
        public static bool IsNullOrWhiteSpace(this string? value, ErrorContext errorContext, out ValidationError nullOrEmptyError)
        {
            ArgumentNullException.ThrowIfNull(errorContext);

            nullOrEmptyError = default!;

            if (string.IsNullOrWhiteSpace(value))
            {
                nullOrEmptyError = ValidationError.Required(errorContext);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="value1"/> and <paramref name="value2"/> do <b>not</b> match
        /// and populates <paramref name="notMatchingError"/>. Returns <see langword="false"/> if they match.
        /// Comparison is ordinal (case-sensitive).
        /// </summary>
        /// <param name="fieldName">Label for <paramref name="value1"/>, used in error descriptions.</param>
        /// <param name="confirmFieldName">Label for <paramref name="value2"/>, used in error descriptions.</param>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <param name="errorContext">The context used to create the validation error.</param>
        /// <param name="notMatchingError">The resulting validation error when the values do not match.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errorContext"/> is <see langword="null"/>.</exception>
        public static bool DoesNotMatch(this string? value1, string? value2, string fieldName, string confirmFieldName, ErrorContext errorContext, out ValidationError notMatchingError)
        {
            ArgumentNullException.ThrowIfNull(errorContext);

            notMatchingError = default!;

            if (value1.IsNullOrWhiteSpace(fieldName, errorContext, out var value1Error))
            {
                notMatchingError = value1Error;
                return true;
            }

            if (value2.IsNullOrWhiteSpace(confirmFieldName, errorContext, out var value2Error))
            {
                notMatchingError = value2Error;
                return true;
            }

            if (!string.Equals(value1, value2, StringComparison.Ordinal))
            {
                notMatchingError = ValidationError.NonMatchingValues(errorContext with { FieldName = fieldName }, confirmFieldName);
                return true;
            }

            return false;
        }

    }
}
