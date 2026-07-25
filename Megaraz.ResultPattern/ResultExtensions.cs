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

    /// <summary>Converts a typed result to its non-generic base result.</summary>
    public static Result ToResult<TValue>(this Result<TValue> result)
        where TValue : notnull
    {
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }

    /// <summary>Converts a failed non-generic result to a typed result.</summary>
    public static Result<TValue> ToResult<TValue>(this Result result)
        where TValue : notnull =>
        Result<TValue>.FromResult(result);

    /// <summary>Asynchronously maps a successful value.</summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> map)
        where TIn : notnull
        where TOut : notnull
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(map);
        return result.IsFailure
            ? Result<TOut>.FromResult(result)
            : Result<TOut>.Success(await (map(result.Value) ?? throw new InvalidOperationException("The map function returned null.")).ConfigureAwait(false));
    }
}
