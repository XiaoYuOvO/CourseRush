namespace CourseRush.Auth;

public interface IAuthDataKey
{
    public string KeyName { get; init; }
    
}

public class CommonDataKey
{
    public static readonly AuthDataKey<string> UserName = new("UserName");
    public static readonly AuthDataKey<string> Password = new("Password");
}

public record AuthDataKey<TData>(string KeyName) : IAuthDataKey where TData : notnull
{
    public override string ToString()
    {
        return $"{KeyName} : {typeof(TData)}";
    }

    public override int GetHashCode()
    {
        return KeyName.GetHashCode();
    }
};