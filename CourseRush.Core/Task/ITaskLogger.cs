namespace CourseRush.Core.Task;

public interface ITaskLogger
{
    void LogInfo(ISelectionTask task, string message);
    void LogWarn(ISelectionTask task, string message);
    void LogError(ISelectionTask task, string message);
    void LogError(ISelectionTask task, string message, BasicError error);
}