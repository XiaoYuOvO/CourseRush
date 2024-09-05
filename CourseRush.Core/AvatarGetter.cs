using Resultful;

namespace CourseRush.Core;

public class AvatarGetter(Func<Result<Task<Stream>, BasicError>> getter)
{
    public Result<Task<Stream>, BasicError> Get()
    {
        return getter();
    }
}