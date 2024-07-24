using Resultful;

namespace CourseRush.Auth;

public class AuthDataTable
{
    private readonly Dictionary<IAuthDataKey, object> _userAuthDataTable = new();

    public Result<TResult, AuthError> RequireData<TKey, TResult>(TKey key) where TKey : AuthDataKey<TResult> where TResult : notnull
    {
        return _userAuthDataTable[key] is TResult ? (TResult)_userAuthDataTable[key] : new AuthError("Cannot find required data for key " + key + " with type " + typeof(TResult));
    }

    public AuthDataTable LimitedView(IEnumerable<IAuthDataKey> keys)
    {
        var authDataTable = new AuthDataTable();
        foreach (var authDataKey in keys)
        {
            authDataTable._userAuthDataTable[authDataKey] = _userAuthDataTable[authDataKey];
        }
        return authDataTable;
    }

    public void MergeReplace(AuthDataTable source)
    {
        foreach (var keyValuePair in source._userAuthDataTable)
        {
            _userAuthDataTable[keyValuePair.Key] = keyValuePair.Value;
        }
    }

    public void UpdateData<TData>(AuthDataKey<TData> key, TData data) where TData : notnull
    {
        _userAuthDataTable[key] = data;
    }
}