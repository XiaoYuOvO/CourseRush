using System.Text.Json.Nodes;

namespace CourseRush.Core;

public interface ISelectionSession
{
    public string SelectionId { get; }
    public string SelectionTimeId { get; }
    public string SelectionTypeName { get; }
    public void AddInfoToJson(JsonObject json);
}