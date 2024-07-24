using CourseRush.Core.Network;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Auth;

public class AuthChain<TResult> where TResult : AuthResult
{

    private readonly AuthNode _finalNode;
    private readonly Func<AuthDataTable, Result<TResult, AuthError>> _resultFactory;

    internal AuthChain(AuthNode finalNode, Func<AuthDataTable, Result<TResult, AuthError>> resultFactory)
    {
        _finalNode = finalNode;
        _resultFactory = resultFactory;
        ValidateNode(finalNode);
    }

    public Result<TResult, AuthError> Auth(UsernamePassword usernamePassword, WebClient client)
    {
        var authDataTable = new AuthDataTable();
        authDataTable.UpdateData(CommonDataKey.UserName, usernamePassword.Username);
        authDataTable.UpdateData(CommonDataKey.Password, usernamePassword.Password);
        return PopulateAuthNode(_finalNode, authDataTable, client).WithResult().Bind(_ => _resultFactory(authDataTable));
    }

    private VoidResult<AuthError> PopulateAuthNode(AuthNode node, AuthDataTable dataTable, WebClient client)
    {
        return node.Requires
            .Aggregate(Result.Ok<AuthError>(), (result, authNode) => result.Bind(_ => PopulateAuthNode(authNode, dataTable, client)))
            .Bind(_ => node.Auth(dataTable, client));
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