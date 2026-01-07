using CourseRush.Core;

namespace CourseRush.HNU;

public class HNUCourseType : CourseType
{
    private static readonly HNUCourseType AcademicCore = new("学门核心", 15);
    private static readonly HNUCourseType MajorCore = new("专业核心", 13);
    private static readonly HNUCourseType CategoryCore = new("学类核心", 41);
    private static readonly HNUCourseType MajorElective = new("专业选修", 2);
    private static readonly HNUCourseType GeneralCompulsory = new("通识必修", 42);
    private static readonly HNUCourseType GeneralElective = new("通识选修", 15);
    private static readonly HNUCourseType PublicElective = new("公共选修课", 06);
    private static readonly HNUCourseType Practice = new("实践环节", 8);



    public static HNUCourseType TypeFromCode(int code)
    {
        return code switch
        {
            2 => MajorElective,
            8 => Practice,
            13 => MajorCore,
            06 => PublicElective,
            41 => CategoryCore,
            15 => GeneralElective,
            58 => GeneralCompulsory,
            _ => GeneralElective
        };
    }
    
    public static HNUCourseType TypeFromName(string name)
    {
        return name switch
        {
            "专业核心" => MajorCore,
            "学类核心" => CategoryCore,
            "必修" => CategoryCore,
            "专业选修" => MajorElective,
            "通识必修" => GeneralCompulsory,
            "选择性必修" => GeneralCompulsory,
            "通识选修" => GeneralElective,
            "公共选修课" => PublicElective,
            "实践环节" => Practice,
            _ => GeneralElective
        };
    }

    protected HNUCourseType(string name, int i) : base(name, i)
    {
    }
}