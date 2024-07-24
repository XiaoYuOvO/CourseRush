using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class CourseSelectClient : HdjwClient
{
    private static readonly Uri GetCoursesByCategoryUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/findKcInfoByfl");
    private static readonly Uri GetCategoriesInRoundUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/findXsxkjdByOne?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findXsxkjdByOne&sf_request_type=ajax ");
    private static readonly Uri SelectCourseUri = new("http://hdjw.hnu.edu.cn/resService/jwxtpt/v1/xsd/stuCourseCenterController/saveStuXk?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.saveStuXk&sf_request_type=ajax");
    private readonly CourseSelection _target;
    public CourseSelectClient(HdjwAuthResult authResult, CourseSelection targetSelection) : base(authResult)
    {
        _target = targetSelection;
    }

    //POST ?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findKcInfoByfl&sf_request_type=ajax 
    public List<HNUCourse> GetCoursesByCategory(CourseCategory category)
    {
        
        return new List<HNUCourse>();
    }
    
    //POST /resService/jwxtpt/v1/xsd/stuCourseCenterController/findXsxkjdByOne?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.findXsxkjdByOne&sf_request_type=ajax 
    public Result<List<CourseCategory>, HdjwError> GetCategoriesInRound()
    {
        return EnsureResultSuccess(
                Post(GetCategoriesInRoundUri, JsonContent.Create(GetBasicJson()))
                    .Bind(response => response.ReadJsonObject()))
            .Bind(json => json.Require("data"))
            .Bind(data => data.Require("xfyqMap"))
            .Bind(xfyqMap => xfyqMap.Require("xkgl011011List")
                .Map(node => node.AsArray().Select(obj => obj?.AsObject()))
                .Bind(objects =>
                    (from jsonObject in objects where jsonObject != null select CourseCategory.FromJson(jsonObject))
                    .CombineResults(HdjwError.Combine)));
    }

    //POST /resService/jwxtpt/v1/xsd/stuCourseCenterController/getUserKb?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.getUserKb&sf_request_type=ajax
    public List<HNUCourse> GetCurrentSelectedCourses()
    {
        return new List<HNUCourse>();
    }
    
    //POST /resService/jwxtpt/v1/xsd/stuCourseCenterController/saveStuXk?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.saveStuXk&sf_request_type=ajax
    public VoidResult<HdjwError> SelectCourse(HNUCourse course)
    {
        var jsonObject = GetBasicJson();
        course.AddCourseSelectionToJson(jsonObject);
        return EnsureResultSuccess(Post(SelectCourseUri, JsonContent.Create(jsonObject)).Bind(response => response.ReadJsonObject())).DiscardValue();
    }
    
    //POST resService/jwxtpt/v1/xsd/stuCourseCenterController/saveTx?resourceCode=XSMH0303&apiCode=jw.xsd.courseCenter.controller.StuCourseCenterController.saveTx&sf_request_type=ajax
    public void RemoveSelectedCourse(HNUCourse course)
    {
        
    }

    private JsonObject GetBasicJson()
    {
        var jsonObject = new JsonObject();
        _target.AddInfoToJson(jsonObject);
        return jsonObject;
    }
}