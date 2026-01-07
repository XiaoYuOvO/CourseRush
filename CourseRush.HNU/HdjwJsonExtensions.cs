using System.Numerics;
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
    
    public static Result<JsonArray, HdjwError> RequireArray(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode[nodeName]?.Ok<JsonNode, HdjwError>().TryBind<JsonNode, HdjwError, JsonArray>(node => node.AsArray(), HdjwError.Wrap) ?? HdjwError.JsonError($"Cannot find required field \"{nodeName}\" in json", jsonNode);
    }

    public static Result<JsonObject, HdjwError> RequireObject(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode[nodeName]?.Ok<JsonNode, HdjwError>().TryBind<JsonNode, HdjwError, JsonObject>(node => node.AsObject(), HdjwError.Wrap) ?? HdjwError.JsonError($"Cannot find required field \"{nodeName}\" in json", jsonNode);
    }

    
    public static Result<IEnumerable<JsonObject>, HdjwError> RequireObjectArray(this JsonArray array)
    {
        return array.Where(node => node is JsonObject).Select(node => node!.AsObject()).Ok<IEnumerable<JsonObject>, HdjwError>();
    }

    public static JsonArray ToJsonArray<TValue>(this IEnumerable<TValue> enumerable,
        Func<TValue, JsonObject> toObjectFunc)
    {
        return new JsonArray((from o in enumerable select toObjectFunc(o)).ToArray<JsonNode>());
    }

    public static Result<string, HdjwError> RequireString(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode[nodeName]?.GetString() ?? HdjwError.JsonError($"Cannot find required string field \"{nodeName}\" in json", jsonNode);
    }

    public static Result<int, HdjwError> ParseInt(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode.RequireString(nodeName).Bind(s => Parse<int>(jsonNode, s));
    }

    public static Result<TNumber, HdjwError> Parse<TNumber>(JsonNode jsonNode, string s) where TNumber : INumberBase<TNumber>
    {
        return TNumber.TryParse(s, provider:null, out var num) ? num.Ok<TNumber, HdjwError>() : HdjwError.JsonError($"Cannot parse {typeof(TNumber)}: {s}", jsonNode);
    }
    
    public static Result<TNumber, HdjwError> Parse<TNumber>(this string s) where TNumber : INumberBase<TNumber>
    {
        return TNumber.TryParse(s, provider:null, out var num) ? num.Ok<TNumber, HdjwError>() : HdjwError.Create($"Cannot parse {typeof(TNumber)}: {s}");
    }

    public static Result<int, HdjwError> ParseInt(this JsonNode jsonNode, string nodeName, string fallback)
    {
        return jsonNode.GetString(nodeName, fallback).Bind(s => int.TryParse(s, out var num) ? num.Ok<int, HdjwError>() : HdjwError.JsonError($"Cannot parse integer", jsonNode));
    }
    
    public static Result<int, HdjwError> ParseInt(this JsonNode jsonNode, string nodeName, int fallbackValue)
    {
        return ParseInt(jsonNode, nodeName).ReturnOrValue(_ => fallbackValue);
    }
    
    public static Result<int, HdjwError> RequireInt(this JsonNode jsonNode, string nodeName)
    {
        return jsonNode.Require(nodeName).Map(node => node.GetValue<int>());
    }
    
    public static Result<int, HdjwError> ParseInt(this JsonNode jsonNode)
    {
        return jsonNode.Ok<JsonNode, HdjwError>().TryMap(node => int.Parse(node.GetValue<string>()), HdjwError.Wrap);
    }
    
    public static Result<int, HdjwError> GetInt(this JsonNode jsonNode, string nodeName, int fallbackValue)
    {
        return jsonNode[nodeName]?.GetValue<int>() ?? fallbackValue;
    }
    
    public static Result<float, HdjwError> GetFloat(this JsonNode jsonNode, string nodeName, float fallbackValue)
    {
        return jsonNode[nodeName]?.GetValue<float>() ?? fallbackValue;
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