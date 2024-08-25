using System.Text.Json.Nodes;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.Core.Task;
using System.Threading.Tasks;

public class RemoveSelectionTask<TError, TCourse>(TCourse course, ITaskLogger? logger = null)
    : SingleSelectionTask<TError, TCourse>(TaskTypes<TError, TCourse>.RemoveSelectionTask, course, logger), IJsonSerializable<RemoveSelectionTask<TError, TCourse>, TaskSerializationError>
    where TError : BasicError, ISelectionError, ICombinableError<TError>
    where TCourse : ICourse, IJsonSerializable<TCourse, TError>
{
    public override string Name => string.Format(Language.task_remove_task, TargetCourse.ToSimpleString());
    protected internal override Task<VoidResult<TError>> DoSelectionTask(ICourseSelector<TError, TCourse> selector)
    {
        return Task.Run(() => selector.RemoveSelectedCourse([TargetCourse]));
    }

    public override void ApplyCourseConflict(TCourse course)
    {
        course.ConflictsCache.Remove(TargetCourse);
    }

    public override void UndoCourseConflict(TCourse course)
    {
        course.TimeTable.ResolveConflictWith(TargetCourse.TimeTable)
            .Tee(results => course.ConflictsCache[TargetCourse] = results);
    }

    public static Result<RemoveSelectionTask<TError, TCourse>, TaskSerializationError> FromJson(JsonObject jsonObject)
    {
        return ReadCourseFromJson(jsonObject)
            .Bind<RemoveSelectionTask<TError, TCourse>>(course => new RemoveSelectionTask<TError, TCourse>(course))
            .BindAction(task => task.ReadFromJson(jsonObject));
    }
    
}