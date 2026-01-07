using System.Collections.Immutable;
using System.Net.Http.Json;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core;
using CourseRush.Core.Network;
using CourseRush.Core.Util;
using Microsoft.ClearScript.JavaScript;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseSelectClient(HdjwAuthResult authResult, HNUSelectionSession targetSelectionSession)
    : HdjwClient(authResult), ICourseSelectionClient<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseCategory>
{
    private const string GetCoursesByCategoryBaseUri = "http://hdjw.hnu.edu.cn/jsxsd/xsxkkc/";
    private static readonly string SelectCourseBaseUri = new("http://hdjw.hnu.edu.cn/jsxsd/xsxkkc/");
    private static readonly Uri RemoveCoursesBaseUri = new("http://hdjw.hnu.edu.cn/jsxsd/xsxkjg/xstkOper");
    private static readonly Uri GetCurrentCourseTableUri = new("http://hdjw.hnu.edu.cn/jsxsd/xsxkjg/comeXkjglb?isktx=true");
    private const string InitializeSelectionUri = "http://hdjw.hnu.edu.cn/jsxsd/xsxk/newXsxkzx";
    private const string InitializeSelection2Uri = "http://hdjw.hnu.edu.cn/jsxsd/xsxk/selectBottom";

    private static readonly Dictionary<string, (string query, string operation)> CourseSelectionQueryOperationMap = new()
    {
        ["bxqxksfkf"] = (query: "xsxkBxqjhxk",operation:"bxqjhxkOper"),
        ["kcbxksfkf"] = (query: "xsxkKcbxk",operation:"kcbxkOper"),
        ["ggksfkf"] = (query: "xsxkGgxxkxk",operation:"ggxxkxkOper"),
        ["kzyxksfkf"] = (query: "xsxkFawxk",operation:"fawxkOper"),
        ["fxzyxksfkf"] = (query: "xsxkFxxk",operation:"fxxkOper"),
        ["cxxkkf"] = (query: "xsxkCxxk",operation:"cxxkOper")
    };
    public async Task<VoidResult<HdjwError>> InitializeSelectionAsync()
    {
        return (await PostAsync(new Uri(targetSelectionSession.ApplyInfoToWebFormHeader(InitializeSelectionUri)), accept:MediaType.Html))
            .DiscardValue()
            .MapError(HdjwError.Wrap);
    }

    //TODO Get max credits
    //TODO Get student time table
    private static readonly Dictionary<string, string> HeaderQuery = new()
    {
        ["kcxx"] = "",
        ["skls"] = "",
        ["skxq"] = "",
        ["skjc"] = "",
        ["endJc"] = "",
        ["sfym"] = "false",
        ["sfct"] = "false",
        ["sfxx"] = "false",
        ["skfs"] = "",
        ["kkdw"] = "",
        ["kctype"] = ""
    };

    private static readonly Dictionary<string, string> ContentQuery = new()
    {
        ["sEcho"] = "1",
        ["iColumns"] = "15",
        ["sColumns"] = "",
        ["iDisplayStart"] = "0",
        ["iDisplayLength"] = "100",
        ["mDataProp_0"] = "jx0404id",
        ["mDataProp_1"] = "kch",
        ["mDataProp_2"] = "kcmc",
        ["mDataProp_3"] = "ktmc",
        ["mDataProp_4"] = "fzmc",
        ["mDataProp_5"] = "xf",
        ["mDataProp_6"] = "skls",
        ["mDataProp_7"] = "sksj",
        ["mDataProp_8"] = "skdd",
        ["mDataProp_9"] = "xqmc",
        ["mDataProp_10"] = "xkrs",
        ["mDataProp_11"] = "syrs",
        ["mDataProp_12"] = "skfsmc",
        ["mDataProp_13"] = "ctsm",
        ["mDataProp_14"] = "czOper"
    };
    public async IAsyncEnumerable<IEnumerable<Result<HNUCourse, HdjwError>>> GetCoursesByCategory(
        HNUCourseCategory category)
    {
        yield return (await PostAsync(new Uri(GetCoursesByCategoryBaseUri + category.CategoryQuery).ApplyWebForms(HeaderQuery),
                new FormUrlEncodedContent(ContentQuery)))
            .MapError(HdjwError.Wrap)
            .Bind(response => response.ReadJsonObject()
                .MapError(HdjwError.Wrap)
                .Bind(json => json.RequireArray("aaData").Bind(array =>
                    array.RequireObjectArray()
                        .Map<IEnumerable<Result<HNUCourse, HdjwError>>>(courses =>
                            courses.Select(course => HNUCourse.FromJson(course, category))))))
            .Match<IEnumerable<Result<HNUCourse, HdjwError>>>(results => results,
                error => error.SingletonEnumerable<HNUCourse, HdjwError>());
    }

    private const string CategoryXPath =
        "//div[@class='selectBottom']//ul[@class='layui-tab-title']/li[@name='xklxLi']";
    public async Task<Result<IReadOnlyList<HNUCourseCategory>, HdjwError>> GetCategoriesInRoundAsync()
    {
        var result = await PostAsync(new Uri($"{targetSelectionSession.ApplyInfoToWebFormHeader(InitializeSelection2Uri)}&sfylxkstr="), new Dictionary<string, string>(), MediaType.Html);
        return result.Bind<IReadOnlyList<HNUCourseCategory>>(response => response.ReadHtml().DocumentNode
                .SelectNodes(CategoryXPath)
                .Select(node => (id: node.GetAttributeValue("id", ""), content: node.InnerText))
                .Where(value => CourseSelectionQueryOperationMap.ContainsKey(value.id))
                .Select(tuple =>
                {
                    var queryOperation = CourseSelectionQueryOperationMap[tuple.id];
                    return new HNUCourseCategory(tuple.id, tuple.content, queryOperation.operation, queryOperation.query);
                }).ToList())
            .MapError(HdjwError.Wrap);
    }


    public Result<IReadOnlyList<HNUSelectedCourse>, HdjwError> GetCurrentCourseTable()
    {
        return Post(GetCurrentCourseTableUri).MapError(HdjwError.Wrap).Bind(response =>
            response.ReadHtml().DocumentNode.SelectNodes("//table[@id='tbData']//tbody/tr") //TODO Process null
                .Select(HNUSelectedCourse.FromHtml).CombineResults());
    }
    
    public VoidResult<HdjwError> SelectCourse(HNUCourse course)
    {
        var webForms = new Dictionary<string, string>();
        course.AddCourseSelectionToWebForms(webForms);
        return EnsureResultSuccess(Post(new Uri(SelectCourseBaseUri + course.Category?.CategoryOperator).ApplyWebForms(webForms))).DiscardValue();
    }
    
    public VoidResult<HdjwError> RemoveSelectedCourse(HNUCourse course)
    {
        Dictionary<string, string> request = new()
        {
            ["jx0404id"] = course.Id
        };
        return EnsureResultSuccess(Post(RemoveCoursesBaseUri.ApplyWebForms(request))).DiscardValue();
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
}