namespace CourseRush.Auth;

public class AuthConvention
{
    internal List<IAuthDataKey> RequiredData { get; } = new();
    internal List<IAuthDataKey> ProvidedData { get; } = new();

    public AuthConvention Provides(params IAuthDataKey[] dataKey)
    {
        ProvidedData.AddRange(dataKey);
        return this;
    }

    public AuthConvention Requires(params IAuthDataKey[] dataKey)
    {
        RequiredData.AddRange(dataKey);
        return this;
    }
}