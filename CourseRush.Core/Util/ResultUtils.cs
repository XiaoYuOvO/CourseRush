using Resultful;

namespace CourseRush.Core.Util;

public static class ResultUtils
{
    public static Result<TResult, TError> TryBind<TValue, TError, TResult>(this Result<TValue, TError> result, Func<TValue, Result<TResult, TError>> mapper, Func<Exception, TError> exceptionWrapper)
    {
        return result.Bind<TResult>(res =>
        {
            try
            {
                return mapper(res);
            }
            catch (Exception ex)

            {
                return exceptionWrapper(ex);
            }
        });
    }
    
    public static Result<TResult, TError> TryMap<TValue, TError, TResult>(this Result<TValue, TError> result, Func<TValue, TResult> mapper, Func<Exception, TError> exceptionWrapper)
    {
        return result.Bind<TResult>(res =>
        {
            try
            {
                return mapper(res);
            }
            catch (Exception ex)

            {
                return exceptionWrapper(ex);
            }
        });
    }

    public static bool IsFail<TResult, TError>(this Result<TResult, TError> result) =>
        result.Match(_ => false, _ => true);
    
    public static bool IsFail<TError>(this VoidResult<TError> result) =>
        result.Match(_ => false, _ => true);
    
    public static TResult GetOrDefault<TResult, TError>(this Result<TResult, TError> result, TResult defaultValue) =>
        result.Match(r => r, _ => defaultValue);
    
    public static Result<IReadOnlyList<TResult>, TError> CombineResults<TResult, TError>(this IEnumerable<Result<TResult, TError>> results) 
        where TError : BasicError, ICombinableError<TError>
    {
        var successes = new List<TResult>();
        var errors = new List<TError>();
        foreach (var result in results)
        {
            var oneOf = result.ToOneOf();
            if (result.IsFail())
            {
                errors.Add(oneOf.AsT1);
            }
            else
            {
                successes.Add(oneOf.AsT0);
            }
        }
        return errors.Count != 0 ? TError.Combine(errors).Fail<IReadOnlyList<TResult>, TError>() : successes.Ok<IReadOnlyList<TResult>, TError>();
    }
    
    public static Result<IReadOnlyList<TResult>, TError> CombineResultsDiscardError<TResult, TError>(this IEnumerable<Result<IReadOnlyList<TResult>, TError>> results) 
        where TError : BasicError, ICombinableError<TError>
    {
        var successes = new List<TResult>();
        foreach (var result in results)
        {
            var oneOf = result.ToOneOf();
            if (!result.IsFail())
            {
                successes.AddRange(oneOf.AsT0);
            }
        }
        return successes.Ok<IReadOnlyList<TResult>, TError>();
    }

    public static async IAsyncEnumerable<Result<TResult, TError>> FlattenAsyncEnumerable<TResult, TError>(
        this Result<IAsyncEnumerable<Result<TResult, TError>>, TError> result)
    {
        var oneOf = result.ToOneOf();
        if (oneOf.IsT1)
        {
            yield return oneOf.AsT1;
        }
        else
        {
            await foreach (var result1 in oneOf.AsT0)
            {
                yield return result1;
            }
        }
    }

    public static IEnumerable<Result<TResult, TError>> SingletonEnumerable<TResult, TError>(this TError error)
    {
        return [error.Fail<TResult, TError>()];
    }

    public static VoidResult<TError> CombineResults<TError>(this IEnumerable<VoidResult<TError>> results) where TError : BasicError, ICombinableError<TError>
    {
        var errors = new List<TError>();
        foreach (var result in results)
        {
            result.TeeError(err => errors.Add(err));
        }
        return errors.Count != 0 ? TError.Combine(errors).Fail() : Result.Ok<TError>();
    }

    public static VoidResult<TError> AcceptOr<TValue, TError>(this Option<TValue?> option, Action<TValue> acceptor, Func<TError> errorSupplier) where TValue : notnull
    {
        
        return option.Match(val =>
        {
            acceptor(val!);
            return Result.Ok<TError>();
        }, _ => errorSupplier());
    }

    public static Result<Unit, TError> WithResult<TError>(this VoidResult<TError> voidResult)
    {
        return voidResult.Map2(e => e, u => u);
    }

    public static Result<TResult, TError> BindAction<TResult, TError>(this Result<TResult, TError> input,
        Func<TResult, VoidResult<TError>> action)
    {
        return input.Bind<TResult>(result => action(result).Match<Result<TResult, TError>>(_ => result, err => err));
    }

    public static IEnumerable<TResult> Presented<TResult>(this IEnumerable<Option<TResult>> options)
    {
        return options.Select(option => option.Match(v => v, _ => (TResult)(object)null!)).Where(result => result != null);
    }
    
    public static IEnumerable<TResult> Presented<TResult>(this IEnumerable<TResult?> options)
    {
        return options.Where(result => result != null).Select(val => val!);
    }
}