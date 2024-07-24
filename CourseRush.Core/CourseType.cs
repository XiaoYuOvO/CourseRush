namespace CourseRush.Core;

public class CourseType
{
    public static readonly CourseType Compulsory = new("选修");
    public static readonly CourseType Elective = new("必修");
    public static readonly CourseType CompulsoryElective = new("选择性必修");

    public string Name { get; }

    protected CourseType(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}