using System.Text.Json.Nodes;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Core.Task;
using System.Threading.Tasks;

public class ParallelSelectionTask<TError, TCourse>(IReadOnlyList<SelectionTask<TError, TCourse>> subTasks, ITaskLogger? logger = null)
    : AggregatedSelectionTask<TError, TCourse>(TaskTypes<TError, TCourse>.ParallelSelectionTask, subTasks, logger), IJsonSerializable<ParallelSelectionTask<TError, TCourse>, TaskSerializationError> where TError : BasicError, ICombinableError<TError>, ISelectionError
    where TCourse : ICourse, IJsonSerializable<TCourse, TError>
{
    public override string Name => string.Format(Language.task_parallel_tasks, SubTasks.Count);
    public override async Task InitializeAndStartTask(ICourseSelector<TError, TCourse> selector)
    {
        LogInfo(Language.task_info_launched);
        Status = TaskStatus.Running;
        await Task.WhenAll(SubTasks.Select(task => task.InitializeAndStartTask(selector)));
        if (SubTasks.Select(task => task.Status).All(status => status == TaskStatus.Completed))
        {
            Status = TaskStatus.Completed;
            LogInfo(Language.task_status_completed);
        }
        else
        {
            Status = TaskStatus.Failed;
            LogInfo(Language.task_log_failed);
        }
    }
    
    protected internal override Task<VoidResult<TError>> DoSelectionTask(ICourseSelector<TError, TCourse> selector)
    {
        return Task.FromResult(Result.Ok<TError>());
    }

    public static Result<ParallelSelectionTask<TError, TCourse>, TaskSerializationError> FromJson(JsonObject jsonObject)
    {
        return ReadSubTasksFromJson(jsonObject)
            .Map(tasks => new ParallelSelectionTask<TError, TCourse>(tasks))
            .BindAction(task => task.ReadFromJson(jsonObject));
    }
}