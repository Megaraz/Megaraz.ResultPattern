namespace Megaraz.ResultPattern;

/// <summary>Helpers for transforming typed results.</summary>
public static class ResultExtensions
{
    /// <summary>Converts a failed result to another value type while preserving its error state.</summary>
    public static Result<TOut> From<TIn, TOut>(this Result<TIn> result)
        where TIn : notnull
        where TOut : notnull
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.IsSuccess)
            throw new InvalidOperationException("Only failed results can be converted with From.");

        return Result<TOut>.FromFailure(result.Message, result.ValidationErrors, result.PrimaryError);
    }

    /// <summary>Maps a successful value and preserves failures.</summary>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> map)
        where TIn : notnull
        where TOut : notnull
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(map);
        return result.IsFailure
            ? result.From<TIn, TOut>()
            : Result<TOut>.Success(map(result.Value));
    }
}
