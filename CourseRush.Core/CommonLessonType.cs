namespace CourseRush.Core;

public class CommonLessonType
{
    public static readonly CommonLessonType Compulsory = new("选修");
    public static readonly CommonLessonType Elective = new("必修");
    public static readonly CommonLessonType CompulsoryElective = new("选择性必修");

    
    private readonly string _name;
    private CommonLessonType(string name)
    {
        _name = name;
    }

    public override string ToString()
    {
        return _name;
    }
}