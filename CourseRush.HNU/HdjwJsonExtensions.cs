using System.Text.Json.Nodes;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public static class HdjwJsonExtensions
{
    public static Result<JsonNode, HdjwError> Require(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode[nodeName]?.Ok<JsonNode, HdjwError>() ?? HdjwError.JsonError($"Cannot find required field \"{nodeName}\" in json", jsonNode);
    }
    
    public static Result<string, HdjwError> RequireString(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode[nodeName]?.GetString() ?? HdjwError.JsonError($"Cannot find required string field \"{nodeName}\" in json", jsonNode);
    }

    public static Result<int, HdjwError> ParseInt(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode.RequireString(nodeName).Bind(s => int.TryParse(s, out var num) ? num.Ok<int, HdjwError>() : HdjwError.JsonError($"Cannot parse integer", jsonNode));
    }
    
    public static Result<int, HdjwError> ParseInt(this JsonNode jsonNode, string nodeName, int fallbackValue)
    {
        return ParseInt(jsonNode, nodeName).ReturnOrValue(_ => fallbackValue);
    }
    
    public static Result<int, HdjwError> RequireInt(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode.Require(nodeName).Map(node => node.GetValue<int>());
    }
    
    public static Result<float, HdjwError> RequireFloat(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode.Require(nodeName).Map(node => node.GetValue<float>());
    }

    public static Result<string, HdjwError> GetString(this JsonNode jsonNode)
    {
        return jsonNode.GetValue<string>();
    }

    public static Result<string, HdjwError> GetString(this JsonNode node, string nodeName, string fallbackValue)
    {
        return node[nodeName]?.GetString() ?? fallbackValue;
    }
}