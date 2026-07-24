namespace Megaraz.ResultPattern;

/// <summary>Helpers for transforming typed results.</summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a successful value and preserves failures.
    /// </summary>
    /// <remarks>
    /// Exceptions thrown by <paramref name="map"/> are not caught and propagate to the caller.
    /// </remarks>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> map)
        where TIn : notnull
        where TOut : notnull
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(map);
        return result.IsFailure
            ? Result<TOut>.FromFailure(result.Message, result.ValidationErrors, result.PrimaryError)
            : Result<TOut>.Success(map(result.Value));
    }
}
