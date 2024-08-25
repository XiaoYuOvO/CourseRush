namespace CourseRush.Core;

public interface ISelectedCourse : ICourse
{
    public SelectionMethod SelectionMethod { get; }   
}

public class SelectionMethod(string name, string localizedName)
{
    public static readonly SelectionMethod Preset = new("preset", Language.course_selection_method_preset);
    public static readonly SelectionMethod SelfSelect = new("selfSelection", Language.course_selection_method_self_select);

    public string Name { get; } = name;
    public string LocalizedName { get; } = localizedName;
}