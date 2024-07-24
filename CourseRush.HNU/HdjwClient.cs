using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CourseRush.Auth;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core;
using CourseRush.Core.Network;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class HdjwClient : AuthClient<HdjwAuthResult>
{
    private static readonly Uri GetOngoingCourseSelectionsUri = new(
        "http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/findXsxkjdList?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findXsxkjdList&sf_request_type=ajax");

    private static readonly Uri IsOnlineUri =
        new("http://hdjw.hnu.edu.cn/sf-webproxy/api/online_status?sf_request_type=ajax");
    public HdjwClient(HdjwAuthResult authResult) : base(authResult)
    {
        
    }

    public Result<List<CourseSelection>, HdjwError> GetOngoingCourseSelections()
    {
        var request = new JsonObject
        {
            ["page"] = new JsonObject
            {
                ["pageIndex"] = 1,
                ["pageSize"] = 30
            }
        };
        
        return EnsureResultSuccess(Post(GetOngoingCourseSelectionsUri, JsonContent.Create(request)).Bind((response => response.ReadJsonObject())))
            .Bind(json => json.Require("data")
                .Bind(data => data.Require("resRmd")
                    .Bind(resRmd => resRmd.Require("items")
                        .Bind<List<CourseSelection>>(items =>
                            (from jsonNode in items.AsArray()
                                where jsonNode != null
                                select CourseSelection.FromJson(jsonNode.AsObject())).ToList()
                            .CombineResults(HdjwError.Combine)))));
    }

    public Result<bool, HdjwError> IsOnline()
    {
        return Get(IsOnlineUri).Bind(response => response.ReadJsonObject()).MapError(HdjwError.Wrap).Bind(json => json.Require("online")).Map(node => node.GetValue<bool>());
    }

    public CourseSelectClient GetSelectionClient(CourseSelection target)
    {
        
        return new CourseSelectClient(Auth, target);
    }
    
    protected static Result<JsonObject, HdjwError> EnsureResultSuccess(Result<JsonObject, WebError> result)
    {
        return result.MapError(HdjwError.Wrap).Bind<JsonObject>(jsonObject =>
        {
            if (!jsonObject.ContainsKey("errorCode"))
            {
                return HdjwError.RequestError("The result didn't contains a error code\n", jsonObject);
            }

            if (!jsonObject["errorCode"]!.GetValue<string>().Equals("success") || !jsonObject.ContainsKey("data"))
            {
                return HdjwError.RequestError("The result code didn't indicates success\n", jsonObject);
            }

            return jsonObject;
        });
    }
}