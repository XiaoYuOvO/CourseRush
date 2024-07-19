using CourseRush.Core.Network;

namespace CourseRush.Auth;

public class AuthChain<TResult> where TResult : AuthResult
{

    private readonly AuthNode _finalNode;
    private readonly Func<AuthDataTable, TResult> _resultFactory;

    internal AuthChain(AuthNode finalNode, Func<AuthDataTable, TResult> resultFactory)
    {
        _finalNode = finalNode;
        _resultFactory = resultFactory;
        ValidateNode(finalNode);
    }

    public TResult Auth(UsernamePassword usernamePassword, WebClient client)
    {
        var authDataTable = new AuthDataTable();
        authDataTable.UpdateData(CommonDataKey.UserName, usernamePassword.Username);
        authDataTable.UpdateData(CommonDataKey.Password, usernamePassword.Password);
        PopulateAuthNode(_finalNode, authDataTable, client);
        return _resultFactory(authDataTable);
    }

    private void PopulateAuthNode(AuthNode node, AuthDataTable dataTable, WebClient client)
    {
        foreach (var nodeRequire in node.Requires)
        {
            PopulateAuthNode(nodeRequire, dataTable, client);
        }
        node.Auth(dataTable, client);
    }

    private static ISet<IAuthDataKey> ValidateNode(AuthNode node)
    {
        var keys = new HashSet<IAuthDataKey>();
        foreach (var nodeParentNode in node.Requires)
        {
            foreach (var authDataKey in ValidateNode(nodeParentNode))
            {
                keys.Add(authDataKey);
            }
        }

        var missingKeys = node.AuthConvention.RequiredData.Where(key => !ReferenceEquals(key, CommonDataKey.Password) && !ReferenceEquals(key, CommonDataKey.UserName) && !keys.Contains(key)).ToList();
        if (missingKeys.Any())
        {
            throw new InvalidAuthChainException($"The node {node} requires {string.Join(", ",missingKeys)} but not provided from the former nodes {string.Join<AuthNode>(",",node.Requires)}");
        }
        node.AuthConvention.ProvidedData.ForEach(key => keys.Add(key));
        return keys;
    }
}