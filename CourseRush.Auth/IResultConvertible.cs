namespace CourseRush.Auth;

public interface IResultConvertible<in TResult, out TOut>
{
    static abstract TOut CreateFromResult(TResult result);
}