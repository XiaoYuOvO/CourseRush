namespace CourseRush.Core;

public class TeachingMethod
{
    public static readonly TeachingMethod ClassTeaching = new("课程授课");
    public static readonly TeachingMethod Practice = new("实践");
    public static readonly TeachingMethod JointPractice = new("集中实践");
    public static readonly TeachingMethod CourseInstruction = new("课程指导");
    public static readonly TeachingMethod PhysicsEducation = new("体育");
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