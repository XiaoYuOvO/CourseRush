namespace CourseRush.Core;

public class TeachingMethod
{
    public static readonly TeachingMethod ClassTeaching = new("课堂授课");
    public static readonly TeachingMethod OnlineTeaching = new("线上慕课");
    private readonly string _name;
    private TeachingMethod(string name)
    {
        _name = name;
    }

    public override string ToString()
    {
        return _name;
    }
}