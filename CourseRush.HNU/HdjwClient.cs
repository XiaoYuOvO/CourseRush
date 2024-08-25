using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using CourseRush.Auth;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core;
using CourseRush.Core.Network;
using CourseRush.Core.Util;
using Resultful;
using WebResponse = CourseRush.Core.Network.WebResponse;

namespace CourseRush.HNU;

public class HdjwClient(HdjwAuthResult authResult)
    : AuthClient<HdjwAuthResult>(authResult), 
        IResultConvertible<HdjwAuthResult, HdjwClient>, 
        ISessionClient<HdjwError, HNUCourseSelection, HNUCourse, HNUSelectedCourse, HNUCourseCategory>
{
    private static readonly Uri GetOngoingCourseSelectionsUri = new(
        "http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/findXsxkjdList?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findXsxkjdList&sf_request_type=ajax");

    private static readonly Uri IsOnlineUri = new("http://hdjw.hnu.edu.cn/sf-webproxy/api/online_status?sf_request_type=ajax");
    private static readonly Uri GetUserInfoUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/jczy/userIndex/findUserDetail?resourceCode=ZYGL08&apiCode=jwPublic.controller.UserIndexController.findUserDetail&sf_request_type=ajax");

    private const string AvatarUriTemplate =
        "http://hdjw.hnu.edu.cn/resService/sys/v1/doc/view/{0}?resourceCode=ZYGL08&apiCode=framework.assembly.doc.controller.SysDocController.view&app=PCWEB&userRoleCode=student&resourceCode=ZYGL08&apiCode=framework.security.controller.SysUserController.roleList";

    public Result<IReadOnlyList<HNUCourseSelection>, HdjwError> GetOngoingCourseSelections()
    {
        var request = new JsonObject
        {
            ["page"] = new JsonObject
            {
                ["pageIndex"] = 1,
                ["pageSize"] = 30
            }
        };
        
        return EnsureResultSuccess(Post(GetOngoingCourseSelectionsUri, JsonContent.Create(request)))
            .Bind(json => json.Require("data")
                .Bind(data => data.Require("resRmd")
                    .Bind(resRmd => resRmd.Require("items")
                        .Bind<IReadOnlyList<HNUCourseSelection>>(items =>
                            (from jsonNode in items.AsArray()
                                where jsonNode != null
                                select HNUCourseSelection.FromJson(jsonNode.AsObject())).ToList()
                            .CombineResults()))));
    }

    public Result<IUserInfo, HdjwError> GetUserInfo()
    {
        return EnsureResultSuccess(Post(GetUserInfoUri, JsonContent.Create(new JsonObject
            {
                ["sctype"] = "hndx",
                ["userType"] = "student"
            })))
            .Bind(userDetailJson => userDetailJson.Require("data")
                .Bind(data => data.Require("xsInfo")
                    .Bind(xsInfo => xsInfo.RequireString("bj_name")
                        .Bind(className => xsInfo.RequireString("xs_name")
                            .Bind(studentName => xsInfo.RequireString("zpdz")
                                .Bind<IUserInfo>(avatarId => new HNUUserInfo(studentName, new AvatarGetter(()=>
                                {
                                    return Get(new Uri(string.Format(AvatarUriTemplate, avatarId)))
                                        .MapError(HdjwError.Wrap)
                                        .Bind<WebResponse>(response => response.GetStatusCode() == HttpStatusCode.OK ? 
                                            response 
                                            : new HdjwError($"Invalid response code from server: {response.GetStatusCode()}, cannot get the avatar"))
                                        .Bind<Task<Stream>>(response => response.ReadStreamAsync()).CastError<BasicError>();
                                }), className)))))));
    }

    public Result<bool, HdjwError> IsOnline()
    {
        return Get(IsOnlineUri).Bind(response => response.ReadJsonObject()).MapError(HdjwError.Wrap).Bind(json => json.Require("online")).Map(node => node.GetValue<bool>());
    }

    public ICourseSelectionClient<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseCategory> GetSelectionClient(HNUCourseSelection target)
    {
        return new HNUCourseSelectClient(Auth, target);
    }
    
    protected static Result<JsonObject, HdjwError> EnsureResultSuccess(Result<WebResponse, WebError> result)
    {
        return result.Bind(response => response.ReadJsonObject()).MapError(HdjwError.Wrap).Bind<JsonObject>(jsonObject =>
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

    public static HdjwClient CreateFromResult(HdjwAuthResult result)
    {
        return new HdjwClient(result);
    }
}