using System.Runtime.CompilerServices;
using CourseRush.Core.Util;

namespace CourseRush.Core.Task;

public class RetryOption(string name, string localizedName, Predicate<ISelectionError> shouldRetry) : Enum<RetryOption>(name)
{
    public static readonly RetryOption AlwaysRetry = Register(new(nameof(AlwaysRetry),Language.task_retry_options_always, _ => true));
    public static readonly RetryOption NoRetry = Register(new(nameof(NoRetry),Language.task_retry_options_never, _ => false ));
    public static readonly RetryOption WhenNotUpToLimit = Register(new RetryOption(nameof(WhenNotUpToLimit),Language.task_retry_options_selection_limit, error => !error.IsStudentLimitsError()));

    public Predicate<ISelectionError> ShouldRetry { get; } = shouldRetry;
    public string LocalizedName => localizedName;

    public override string ToString() => LocalizedName;
}