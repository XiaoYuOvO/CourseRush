using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core;
using CourseRush.Core.Util;
using Microsoft.ClearScript.JavaScript;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseSelectClient : HdjwClient, ICourseSelectionClient<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseCategory>{
    private static readonly Uri GetCoursesByCategoryUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/findKcInfoByfl?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findKcInfoByfl&sf_request_type=ajax");
    private static readonly Uri GetCategoriesInRoundUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/findZxxkByEntry?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findZxxkByEntry");
    private static readonly Uri SelectCourseUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/saveStuXk?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.saveStuXk&sf_request_type=ajax");
    private static readonly Uri RemoveCoursesUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/saveTx?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.saveTx&sf_request_type=ajax");
    private static readonly Uri GetCurrentCourseTableUri = new("http://hdjw.hnu.edu.cn//resService/jwxtpt/v1/xsd/stuCourseCenterController/findXkResList?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findXkResList");
    private readonly HNUSelectionSession _target;
    public HNUCourseSelectClient(HdjwAuthResult authResult, HNUSelectionSession targetSelectionSession) : base(authResult)
    {
        _target = targetSelectionSession;
    }
    
    //TODO Get max credits
    //TODO Get student time table
    //POST ?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findKcInfoByfl&sf_request_type=ajax 
    public async IAsyncEnumerable<IEnumerable<Result<HNUCourse, HdjwError>>> GetCoursesByCategory(HNUCourseCategory category)
    {
        await foreach (var enumerable in EnsureResultSuccess(await PostAsync(GetCoursesByCategoryUri, JsonContent.Create(GetBasicJson(new JsonObject
                           {
                               ["page"] = new JsonObject
                               {
                                   ["pageIndex"] = 0,
                                   ["pageSize"] = 0,
                                   ["orderBy"] = "",
                                   ["conditions"] = "QZDATASOFT"
                               },
                               ["xkfl"] = category.GetSubcategoriesJson(),
                               ["xklbbh"] = category.CategoryNumber.ToString()
                           }))))
                           .Bind(result => result.Require("data")
                               .Bind(data => data.RequireArray("showKclist").Bind(kclist =>
                               {
                                   if (kclist.Count != 0)
                                       return kclist.RequireObjectArray().Map(ReadCourses)
                                           .Map(courses => courses.AsAsyncEnumerable());
                                   //Adapt for out of plan selections
                                   return data.RequireObject("kcToNameMap")
                                       .Map(array => from node in array select node.Key)
                                       .Map(ids => GetSubCoursesInCategory(ids, category));
                               }))).Match(results => results, error => error.SingletonEnumerable<HNUCourse, HdjwError>().AsAsyncEnumerable()))
        {
            yield return enumerable;
        }
    }

    private static IEnumerable<Result<HNUCourse, HdjwError>> ReadCourses(IEnumerable<JsonObject> json)
    {
        return from jsonObject in json select HNUCourse.FromJson(jsonObject);
    }

    private async IAsyncEnumerable<IEnumerable<Result<HNUCourse, HdjwError>>> GetSubCoursesInCategory(IEnumerable<string> ids, HNUCourseCategory category)
    {
        foreach (var jczy010Id in ids)
        {
            yield return EnsureResultSuccess(await PostAsync(GetCoursesByCategoryUri,
                                   JsonContent.Create(GetBasicJson(new JsonObject
                                   {
                                       ["page"] = new JsonObject
                                       {
                                           ["pageIndex"] = 0,
                                           ["pageSize"] = 0,
                                           ["orderBy"] = "",
                                           ["conditions"] = "QZDATASOFT"
                                       },
                                       ["jczy010id"] = jczy010Id,
                                       ["from"] = "sxxkrs",
                                       ["xkfl"] = category.GetSubcategoriesJson(),
                                       ["xklbbh"] = category.CategoryNumber.ToString()
                                   }))))
                               .Bind(result => result.Require("data")
                                   .Bind(data => data.RequireArray("showKclist")
                                       .Bind(kclist => kclist.RequireObjectArray().Map(ReadCourses))))
                               .Match(courses => courses, ResultUtils.SingletonEnumerable<HNUCourse, HdjwError>);
        }
    }

    //POST /resService/jwxtpt/v1/xsd/stuCourseCenterController/findXsxkjdByOne?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findXsxkjdByOne&sf_request_type=ajax 
    public Result<IReadOnlyList<HNUCourseCategory>, HdjwError> GetCategoriesInRound()
    {
        return EnsureResultSuccess(Post(GetCategoriesInRoundUri, JsonContent.Create(GetBasicJson())))
            .Bind(json => json.Require("data"))
            .Bind(data => data.RequireArray("showXklbList")
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
                .Bind(data => data.RequireArray("kcCahe")
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
                    ["jczy010id"] = hnuCourse.Jczy010Id,
                    ["kcbh"] = hnuCourse.Code,
                    ["xkfscode"] = "1",
                    ["zycode"] = null
                }))
        };
        return EnsureResultSuccess(Post(RemoveCoursesUri, JsonContent.Create(request))).DiscardValue();
    }

    public Result<WeekTimeTable, HdjwError> GetWeekTimeTable()
    {
        return new WeekTimeTable(ImmutableList.CreateRange(new List<LessonTime>
        {
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50)),
            new(new TimeOnly(8, 0), new TimeOnly(8, 50))
        }));
    }

    private JsonObject GetBasicJson(JsonObject? inObject = null)
    {
        var infoObject = inObject ?? new JsonObject();
        _target.AddInfoToJson(infoObject);
        return infoObject;
    }
}