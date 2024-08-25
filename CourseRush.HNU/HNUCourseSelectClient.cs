using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseSelectClient : HdjwClient, ICourseSelectionClient<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseCategory>{
    private static readonly Uri GetCoursesByCategoryUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/findKcInfoByfl?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findKcInfoByfl&sf_request_type=ajax");
    private static readonly Uri GetCategoriesInRoundUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/findXsxkjdByOne?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findXsxkjdByOne&sf_request_type=ajax ");
    private static readonly Uri SelectCourseUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/saveStuXk?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.saveStuXk&sf_request_type=ajax");
    private static readonly Uri RemoveCoursesUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/saveTx?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.saveTx&sf_request_type=ajax");
    private static readonly Uri GetCurrentCourseTableUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/getUserKb?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.getUserKb&sf_request_type=ajax");
    private readonly HNUCourseSelection _target;
    public HNUCourseSelectClient(HdjwAuthResult authResult, HNUCourseSelection targetSelection) : base(authResult)
    {
        _target = targetSelection;
    }
    
    //TODO Get max credits
    //TODO Get student time table
    //POST ?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findKcInfoByfl&sf_request_type=ajax 
    public Result<IReadOnlyList<HNUCourse>, HdjwError> GetCoursesByCategory(HNUCourseCategory category)
    {
        return EnsureResultSuccess(Post(GetCoursesByCategoryUri, JsonContent.Create(GetBasicJson(new JsonObject
            {
                ["page"] = new JsonObject
                {
                    ["pageIndex"] = 0,
                    ["pageSize"] = 0,
                    ["orderBy"] = "",
                    ["conditions"] = ""
                },
                ["xkfl"] = new JsonArray(),
                ["xklbbh"] = category.CategoryNumber.ToString()
            }))))
            .Bind(result => result.Require("data")
                .Bind(data => data.RequireArray("showKclist")
                    .Bind(kclist => kclist.RequireObjectArray()
                        .Bind(courses =>
                            (from course in courses select HNUCourse.FromJson(course))
                            .CombineResults()))));
    }
    
    //POST /resService/jwxtpt/v1/xsd/stuCourseCenterController/findXsxkjdByOne?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findXsxkjdByOne&sf_request_type=ajax 
    public Result<IReadOnlyList<HNUCourseCategory>, HdjwError> GetCategoriesInRound()
    {
        return EnsureResultSuccess(Post(GetCategoriesInRoundUri, JsonContent.Create(GetBasicJson())))
            .Bind(json => json.Require("data"))
            .Bind(data => data.Require("xfyqMap"))
            .Bind(xfyqMap => xfyqMap.RequireArray("xkgl011011List")
                .Bind(array => array.RequireObjectArray()
                    .Bind(objects =>
                        (from jsonObject in objects where jsonObject != null select HNUCourseCategory.FromJson(jsonObject))
                        .CombineResults())));
    }

    //POST /resService/jwxtpt/v1/xsd/stuCourseCenterController/getUserKb?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.getUserKb&sf_request_type=ajax
    public Result<IReadOnlyList<HNUSelectedCourse>, HdjwError> GetCurrentCourseTable()
    {
        return EnsureResultSuccess(Post(GetCurrentCourseTableUri, JsonContent.Create(GetBasicJson())))
            .Bind(json => json.Require("data")
                .Bind(data => data.RequireArray("resList")
                    .Bind(array => array.RequireObjectArray()
                        .Bind(objects => (from jsonObject in objects select HNUSelectedCourse.FromJson(jsonObject))
                            .CombineResults()))));
    }
    
    //POST /resService/jwxtpt/v1/xsd/stuCourseCenterController/saveStuXk?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.saveStuXk&sf_request_type=ajax
    public VoidResult<HdjwError> SelectCourse(HNUCourse course)
    {
        var jsonObject = GetBasicJson();
        course.AddCourseSelectionToJson(jsonObject);
        return EnsureResultSuccess(Post(SelectCourseUri, JsonContent.Create(jsonObject))).DiscardValue();
    }
    
    //POST resService/jwxtpt/v1/xsd/stuCourseCenterController/saveTx?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.saveTx&sf_request_type=ajax
    public VoidResult<HdjwError> RemoveSelectedCourse(IReadOnlyList<HNUCourse> course)
    {
        var request = new JsonObject
        {
            ["txList"] = course.ToJsonArray(hnuCourse => GetBasicJson(new JsonObject
                {
                    ["id"] = hnuCourse.Id,
                    ["kcbh"] = hnuCourse.Code,
                    ["jczy010id"] = hnuCourse.Jczy010Id
                }))
        };
        return EnsureResultSuccess(Post(RemoveCoursesUri, JsonContent.Create(request))).DiscardValue();
    }

    public Result<WeekTimeTable, HdjwError> GetWeekTimeTable()
    {
        return new WeekTimeTable(ImmutableList<LessonTime>.Empty);
    }

    private JsonObject GetBasicJson(JsonObject? inObject = null)
    {
        var infoObject = inObject ?? new JsonObject();
        _target.AddInfoToJson(infoObject);
        return infoObject;
    }
}