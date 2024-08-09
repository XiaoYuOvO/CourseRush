namespace CourseRush.Core;

public abstract class CourseType
{
    public string Name { get; }
    public int Code { get; }

    protected CourseType(string name, int code)
    {
        Name = name;
        Code = code;
    }

    public override string ToString()
    {
        return Name;
    }
}