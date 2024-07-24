using System.Text.Json.Nodes;
using Resultful;

namespace CourseRush.HNU;

public class CourseSelection
{
    public string SelectionId { get; }
    public string SelectionTimeId { get; }
    public string SelectionTypeName { get; }
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public string SelectionStage { get; }
    public DateTime DailyStartTime { get; }
    public DateTime DailyEndTime { get; }

    private CourseSelection(string selectionId, string selectionTimeId, string selectionTypeName, DateTime startTime, DateTime endTime, string selectionStage, DateTime dailyStartTime, DateTime dailyEndTime)
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

    public static Result<CourseSelection, HdjwError> FromJson(JsonObject jsonObject)
    {
        return jsonObject.RequireString("id")
            .Bind(id => jsonObject.RequireString("jczy013id")
                .Bind(jczy013Id => jsonObject.RequireString("hd_name")
                    .Bind(hdName => jsonObject.RequireString("xkkssj").Map(DateTime.Parse)
                        .Bind(xkkssj => jsonObject.RequireString("xkjssj").Map(DateTime.Parse)
                            .Bind(xkjssj => jsonObject.RequireString("xkfs_name")
                                .Bind(xkfsName => jsonObject.RequireString("mtkssj").Map(DateTime.Parse)
                                    .Bind(mtkssj => jsonObject.RequireString("mtjssj").Map(DateTime.Parse)
                                        .Bind<CourseSelection>(mtjssj => 
                                            new CourseSelection(id, jczy013Id, hdName, xkkssj, xkjssj, xkfsName, mtkssj, mtjssj)))))))));
    }


    public override string ToString()
    {
        return $"{nameof(SelectionId)}: {SelectionId}, {nameof(SelectionTimeId)}: {SelectionTimeId}, {nameof(SelectionTypeName)}: {SelectionTypeName}, {nameof(StartTime)}: {StartTime}, {nameof(EndTime)}: {EndTime}, {nameof(SelectionStage)}: {SelectionStage}, {nameof(DailyStartTime)}: {DailyStartTime}, {nameof(DailyEndTime)}: {DailyEndTime}";
    }

    public void AddInfoToJson(JsonObject json)
    {
        json["xkgl017id"] = SelectionId;
        json["id"] = SelectionId;
        json["xkgl019id"] = SelectionId;
        json["jczy013id"] = SelectionTimeId;
    }
}