using CourseRush.Core.Util;

namespace CourseRush.Core.Task;

public class TaskStatus(string name, string localizedName) : Enum<TaskStatus>(name)
{
    public static readonly TaskStatus Running = Register(new(nameof(Running),Language.task_status_running));
    public static readonly TaskStatus Waiting = Register(new(nameof(Waiting),Language.task_status_waiting));
    public static readonly TaskStatus Completed = Register(new(nameof(Completed),Language.task_status_completed));
    public static readonly TaskStatus Failed = Register(new(nameof(Failed),Language.task_status_failed));
    public static readonly TaskStatus Cancelled = Register(new(nameof(Cancelled),Language.task_status_cancelled));
    public static readonly TaskStatus Paused = Register(new(nameof(Paused),Language.task_status_paused));
    public static readonly TaskStatus Next = Register(new(nameof(Next),Language.task_status_next));
    
    public string LocalizedName => localizedName;

    public override string ToString()
    {
        return LocalizedName;
    }

    public bool IsFinished()
    {
        return this == Failed || this == Completed || this == Cancelled;
    }
}