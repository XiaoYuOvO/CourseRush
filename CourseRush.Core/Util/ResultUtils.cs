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

    public static bool IsFail<TResult, TError>(this Result<TResult, TError> result) =>
        result.Match(_ => false, _ => true);
    
    public static TResult GetOrDefault<TResult, TError>(this Result<TResult, TError> result, TResult defaultValue) =>
        result.Match(r => r, _ => defaultValue);
    
    public static Result<List<TResult>, TError> CombineResults<TResult, TError>(this IEnumerable<Result<TResult, TError>> results, Func<IList<TError>, TError> errorCombinator)
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
        return errors.Any() ? Result.Fail<List<TResult>, TError>(errorCombinator(errors)) : successes.Ok<List<TResult>, TError>();
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
}