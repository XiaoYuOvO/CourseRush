namespace CourseRush.Core;

public class ExaminationMethod
{
    public static readonly ExaminationMethod Examination = new("考试");
    public static readonly ExaminationMethod Evaluation = new("考查");
    public static readonly ExaminationMethod Empty = new("无");
    private readonly string _name;
    private ExaminationMethod(string name)
    {
        _name = name;
    }

    public override string ToString()
    {
        return _name;
    }
}