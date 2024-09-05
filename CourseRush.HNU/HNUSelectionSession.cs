using System.Text.Json.Nodes;
using CourseRush.Core;
using CourseRush.Core.Util;
using Resultful;
using static CourseRush.Core.PresentedData<CourseRush.HNU.HNUSelectionSession>;
namespace CourseRush.HNU;

public class HNUSelectionSession : ISelectionSession, IPresentedDataProvider<HNUSelectionSession>, IJsonSerializable<HNUSelectionSession, HdjwError>
{
    public string SelectionId { get; }
    public string SelectionTimeId { get; }
    public string SelectionTypeName { get; }
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public string SelectionStage { get; }
    public TimeOnly DailyStartTime { get; }
    public TimeOnly DailyEndTime { get; }
    private readonly string xkgl017id; 

    internal HNUSelectionSession(string selectionId, string selectionTimeId, string selectionTypeName, DateTime startTime, DateTime endTime, string selectionStage, TimeOnly dailyStartTime, TimeOnly dailyEndTime, string xkgl017Id)
    {
        SelectionId = selectionId;
        SelectionTimeId = selectionTimeId;
        SelectionTypeName = selectionTypeName;
        StartTime = startTime;
        EndTime = endTime;
        SelectionStage = selectionStage;
        DailyStartTime = dailyStartTime;
        DailyEndTime = dailyEndTime;
        xkgl017id = xkgl017Id;
    }

    public static Result<HNUSelectionSession, HdjwError> FromJson(JsonObject jsonObject)
    {
        return jsonObject.RequireString("id")
            .Bind(id => jsonObject.RequireString("jczy013id")
                .Bind(jczy013Id => jsonObject.RequireString("hd_name")
                    .Bind(hdName => jsonObject.RequireString("xkkssj").TryMap(DateTime.Parse, HdjwError.Wrap)
                        .Bind(xkkssj => jsonObject.RequireString("xkjssj").TryMap(DateTime.Parse, HdjwError.Wrap)
                            .Bind(xkjssj => jsonObject.RequireString("xkfs_name")
                                .Bind(xkfsName => jsonObject.RequireString("mtkssj").TryMap(TimeOnly.Parse, HdjwError.Wrap)
                                    .Bind(mtkssj => jsonObject.RequireString("mtjssj").TryMap(TimeOnly.Parse, HdjwError.Wrap)
                                        .Bind<HNUSelectionSession>(mtjssj => jsonObject.RequireString("xkgl017id")
                                            .Bind<HNUSelectionSession>(xkgl017id => new HNUSelectionSession(id, jczy013Id, hdName, xkkssj, xkjssj, xkfsName, mtkssj, mtjssj, xkgl017id))))))))));
    }

    public JsonObject ToJson()
    {
        return new JsonObject
        {
            ["id"] = SelectionId,
            ["jczy013id"] = SelectionTimeId,
            ["hd_name"] = SelectionTypeName,
            ["xkkssj"] = StartTime.ToString("yyyy-M-d"),
            ["xkjssj"] = EndTime.ToString("yyyy-M-d"),
            ["xkfs_name"] = SelectionStage,
            ["mtkssj"] = DailyStartTime.ToString("HH:mm:ss"),
            ["mtjssj"] = DailyEndTime.ToString("HH:mm:ss")
        };
    }


    public override string ToString()
    {
        return $"{nameof(SelectionId)}: {SelectionId}, {nameof(SelectionTimeId)}: {SelectionTimeId}, {nameof(SelectionTypeName)}: {SelectionTypeName}, {nameof(StartTime)}: {StartTime}, {nameof(EndTime)}: {EndTime}, {nameof(SelectionStage)}: {SelectionStage}, {nameof(DailyStartTime)}: {DailyStartTime}, {nameof(DailyEndTime)}: {DailyEndTime}";
    }

    private static readonly List<PresentedData<HNUSelectionSession>> PresentedData =
    [
        OfString("course_selection.selection_term", selection => selection.SelectionTimeId),
        OfString("course_selection.selection_name", selection => selection.SelectionTypeName),
        OfString("course_selection.start_time", selection => $"{selection.StartTime:yyyy-MM-dd}"),
        OfString("course_selection.end_time", selection => $"{selection.EndTime:yyyy-MM-dd}"),
        OfString("course_selection.daily_start_time", selection => $"{selection.DailyStartTime:t}"),
        OfString("course_selection.daily_end_time", selection => $"{selection.DailyEndTime:t}")
    ];

    public static List<PresentedData<HNUSelectionSession>> GetPresentedData()
    {
        return PresentedData;
    }

    public static List<PresentedData<HNUSelectionSession>> GetSimplePresentedData()
    {
        return PresentedData;
    }

    public void AddInfoToJson(JsonObject json)
    {
        if (!json.ContainsKey("id"))
        {
            json["id"] = SelectionId; 
        }
        json["xkgl017id"] = xkgl017id;
        json["xkgl019id"] = SelectionId;
        json["jczy013id"] = SelectionTimeId;
    }
}