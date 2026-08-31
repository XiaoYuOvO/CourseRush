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

    public async Task<Result<TResult, AuthError>> Auth(UsernamePassword usernamePassword, WebClient client, IAuthInteractive interactive)
    {
        var authDataTable = new AuthDataTable(interactive);
        authDataTable.UpdateData(CommonDataKey.UserName, usernamePassword.Username);
        authDataTable.UpdateData(CommonDataKey.Password, usernamePassword.Password);
        return (await PopulateAuthNode(_finalNode, authDataTable, client)).WithResult().Bind(_ => _resultFactory(authDataTable));
    }

    private async Task<VoidResult<AuthError>> PopulateAuthNode(AuthNode node, AuthDataTable dataTable, WebClient client)
    {
        var result = Result.Ok<AuthError>();
        foreach (var nodeRequire in node.Requires)
        {
            result = await result.BindAsync(_ => PopulateAuthNode(nodeRequire, dataTable, client));
        }

        return await result.BindAsync(_ => node.Auth(dataTable, client));
        // return await (node.Requires
        //     .Aggregate(Result.Ok<AuthError>(), (result, authNode) => await result.BindAsync(_ => PopulateAuthNode(authNode, dataTable, client))))
        //     .BindAsync(_ => node.Auth(dataTable, client));
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