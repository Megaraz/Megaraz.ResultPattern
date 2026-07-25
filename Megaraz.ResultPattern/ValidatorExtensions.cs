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
        /// Returns the required-value validation error, or <see langword="null"/> when
        /// <paramref name="value"/> contains content.
        /// </summary>
        public static ValidationError? ValidateRequired(
            this string? value,
            ErrorContext errorContext,
            string? fieldName = null)
        {
            ArgumentNullException.ThrowIfNull(errorContext);

            if (!string.IsNullOrWhiteSpace(value))
                return null;

            var resolvedFieldName = string.IsNullOrWhiteSpace(fieldName)
                ? errorContext.FieldName
                : fieldName;

            return ValidationError.Required(errorContext with { FieldName = resolvedFieldName });
        }

        /// <summary>
        /// Returns required-value validation errors in input order.
        /// </summary>
        public static IReadOnlyList<ValidationError> ValidateRequiredFields(
            this IEnumerable<(string FieldName, string? Value)> requiredValues,
            ErrorContext errorContext)
        {
            ArgumentNullException.ThrowIfNull(requiredValues);
            ArgumentNullException.ThrowIfNull(errorContext);

            var errors = new List<ValidationError>();
            foreach (var (fieldName, value) in requiredValues)
            {
                var error = value.ValidateRequired(errorContext, fieldName);
                if (error is not null)
                    errors.Add(error);
            }

            return errors;
        }

        /// <summary>
        /// Returns a validation error when the values do not match, or
        /// <see langword="null"/> when they match. Required checks are performed first.
        /// </summary>
        public static ValidationError? ValidateDoesNotMatch(
            this string? value1,
            string? value2,
            string? fieldName,
            string? confirmFieldName,
            ErrorContext errorContext)
        {
            ArgumentNullException.ThrowIfNull(errorContext);

            var value1Error = value1.ValidateRequired(errorContext, fieldName);
            if (value1Error is not null)
                return value1Error;

            var value2Error = value2.ValidateRequired(errorContext, confirmFieldName);
            if (value2Error is not null)
                return value2Error;

            if (string.Equals(value1, value2, StringComparison.Ordinal))
                return null;

            var resolvedFieldName = string.IsNullOrWhiteSpace(fieldName)
                ? errorContext.FieldName
                : fieldName;
            var resolvedConfirmFieldName = string.IsNullOrWhiteSpace(confirmFieldName)
                ? errorContext.FieldName
                : confirmFieldName;

            return ValidationError.NonMatchingValues(
                errorContext with { FieldName = resolvedFieldName },
                resolvedConfirmFieldName);
        }

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

            validationErrors = requiredValues.ValidateRequiredFields(errorContext);
            return validationErrors.Count > 0;
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

            nullOrEmptyError = value.ValidateRequired(errorContext, fieldName)!;
            return nullOrEmptyError is not null;
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

            nullOrEmptyError = value.ValidateRequired(errorContext)!;
            return nullOrEmptyError is not null;
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

            notMatchingError = value1.ValidateDoesNotMatch(
                value2, fieldName, confirmFieldName, errorContext)!;
            return notMatchingError is not null;
        }

    }
}
