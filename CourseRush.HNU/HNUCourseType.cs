using CourseRush.Core;

namespace CourseRush.HNU;

public class HNUCourseType : CourseType
{
    private static readonly HNUCourseType AcademicCore = new("学门核心");
    private static readonly HNUCourseType MajorCore = new("专业核心");
    private static readonly HNUCourseType CategoryCore = new("学类核心");
    private static readonly HNUCourseType MajorElective = new("专业选修");
    private static readonly HNUCourseType GeneralCompulsory = new("通识必修");
    private static readonly HNUCourseType GeneralElective = new("通识选修");
    private static readonly HNUCourseType Practice = new("实践环节");


    public static HNUCourseType TypeFromCode(int code)
    {
        return code switch
        {
            2 => MajorElective,
            8 => Practice,
            13 => MajorCore,
            15 => AcademicCore,
            41 => CategoryCore,
            42 => GeneralElective,
            58 => GeneralCompulsory,
            _ => GeneralElective
        };
    }

    protected HNUCourseType(string name) : base(name) { }
}