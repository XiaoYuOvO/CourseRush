using System.Net;
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

    internal HNUSelectionSession(string selectionId, string selectionTimeId, string selectionTypeName, DateTime startTime, DateTime endTime, string selectionStage, TimeOnly dailyStartTime, TimeOnly dailyEndTime)
    {
        SelectionId = selectionId;
        SelectionTimeId = selectionTimeId;
        SelectionTypeName = selectionTypeName;
        StartTime = startTime;
        EndTime = endTime;
        SelectionStage = selectionStage;
        DailyStartTime = dailyStartTime;
        DailyEndTime = dailyEndTime;
    }

    public static Result<HNUSelectionSession, HdjwError> FromJson(JsonObject jsonObject)
    {
        return jsonObject.RequireString("jx0502zbid")
            .Bind(jx0502zbid => jsonObject.RequireString("xnxq01id")
                .Bind(jczy013Id => jsonObject.RequireString("xklc_mc")
                    .Bind(hdName => jsonObject.RequireString("xkkssj").TryMap(DateTime.Parse, HdjwError.Wrap)
                        .Bind(xkkssj => jsonObject.RequireString("xkjzsj").TryMap(DateTime.Parse, HdjwError.Wrap)
                            .Bind(xkjssj => jsonObject.RequireString("txkzmc")
                                .Bind(xkfsName => jsonObject.RequireString("mtxkkssj").TryMap(TimeOnly.Parse, HdjwError.Wrap)
                                    .Bind(mtkssj => jsonObject.RequireString("mtxkjssj").TryMap(TimeOnly.Parse, HdjwError.Wrap)
                                            .Bind<HNUSelectionSession>(mtjssj => new HNUSelectionSession(jx0502zbid, jczy013Id, hdName, xkkssj, xkjssj, xkfsName, mtkssj, mtjssj)))))))));
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
        json["xkgl019id"] = SelectionId;
        json["jczy013id"] = SelectionTimeId;
    }

    public string ApplyInfoToWebFormHeader(string request)
    {
        return $"{request}?jx0502zbid={SelectionId}";
    }
}