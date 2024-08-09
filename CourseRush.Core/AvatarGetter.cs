using Resultful;

namespace CourseRush.Core;

public class AvatarGetter
{
    private readonly Func<Result<Task<Stream>, BasicError>> _getter;

    public AvatarGetter(Func<Result<Task<Stream>, BasicError>> getter)
    {
        _getter = getter;
    }

    public Result<Task<Stream>, BasicError> Get()
    {
        return _getter();
    }
}