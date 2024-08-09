namespace CourseRush.Core;

public interface ISelectedCourse : ICourse
{
    public SelectionMethod SelectionMethod { get; }   
}

public class SelectionMethod
{
    public static readonly SelectionMethod Preset = new("Preset");
    public static readonly SelectionMethod SelfSelection = new("SelfSelection");
    
    public SelectionMethod(string name)
    {
        Name = name;
    }

    public string Name { get; }
}