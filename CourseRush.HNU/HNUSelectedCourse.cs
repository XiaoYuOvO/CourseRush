using System.Text.Json.Nodes;
using CourseRush.Core;
using Resultful;
using static CourseRush.Core.PresentedData<CourseRush.HNU.HNUSelectedCourse>;

namespace CourseRush.HNU;

public class HNUSelectedCourse(
    int selectedStudentCount,
    int totalStudentCount,
    float totalLearningHours,
    float totalCredits,
    string teacherName,
    string className,
    string courseName,
    string offerInstitution,
    string campus,
    CourseTimeTable timeTable,
    TeachingMethod teachingMethod,
    CourseType courseType,
    ExaminationMethod examinationMethod,
    string id,
    string code,
    string jczy010Id,
    string electiveCourseType,
    string groupName,
    SelectionMethod selectionMethod)
    : HNUCourse(selectedStudentCount, totalStudentCount, totalLearningHours, totalCredits, teacherName, className,"",
        courseName, offerInstitution, campus, timeTable, teachingMethod, courseType, examinationMethod, id, code,
        jczy010Id, electiveCourseType, groupName), ISelectedCourse, IPresentedDataProvider<HNUSelectedCourse>
{
    public SelectionMethod SelectionMethod { get; } = selectionMethod;

    public new static Result<HNUSelectedCourse, HdjwError> FromJson(JsonObject node)
    {
        return node.GetInt("xkrs", 0)
                .Bind<HNUSelectedCourse>(xkrs => node.GetInt("pkrs", 0)
                    .Bind<HNUSelectedCourse>(pkrs => node.RequireFloat("zxs")
                        .Bind<HNUSelectedCourse>(zxs => node.RequireFloat("zxf")
                            .Bind<HNUSelectedCourse>(zxf => node.GetString("skls_name", "无")
                                .Bind<HNUSelectedCourse>(sklsName => node.RequireString("ktmc_name")
                                    .Bind<HNUSelectedCourse>(ktmcName => node.RequireString("kcmc_name")
                                        .Bind<HNUSelectedCourse>(kcmcName => node.RequireString("kkdw_name")
                                            .Bind<HNUSelectedCourse>(kkdwName => node.RequireString("xq_name")
                                                .Bind<HNUSelectedCourse>(xqName => node.GetString("kbinfo", "")
                                                    .Bind(HNUCourseTimeTable.FromString)
                                                    .Bind<HNUSelectedCourse>(table => node.ParseInt("skfscode")
                                                        .Map(IdToTeacherMethod)
                                                        .Bind<HNUSelectedCourse>(teachingMethod => node.ParseInt("kclbcode")
                                                            .Map(HNUCourseType.TypeFromCode)
                                                            .Bind<HNUSelectedCourse>(courseType => node.ParseInt("khfscode", 0)
                                                                .Map(IdToExaminationMethod)
                                                                .Bind<HNUSelectedCourse>(examinationMethod => node
                                                                    .RequireString("id")
                                                                    .Bind<HNUSelectedCourse>(id => node.RequireString("kcbh")
                                                                        .Bind<HNUSelectedCourse>(kcbh => node
                                                                            .GetString("jczy010id", "")
                                                                            .Bind<HNUSelectedCourse>(jczy010Id => node
                                                                                .GetString("szkclb_name", "无")
                                                                                .Bind<HNUSelectedCourse>(szkclb => node
                                                                                    .ParseInt("xkfscode","0").Bind(SelectionMethodFromId)
                                                                                    .Bind<HNUSelectedCourse>(method => node
                                                                                        .GetString("fzmc_name", "")
                                                                                        .Bind<HNUSelectedCourse>(
                                                                                            groupName =>
                                                                                                new HNUSelectedCourse(
                                                                                                    xkrs, 
                                                                                                    pkrs, 
                                                                                                    zxs,
                                                                                                    zxf, 
                                                                                                    sklsName,
                                                                                                    ktmcName, 
                                                                                                    kcmcName,
                                                                                                    kkdwName,
                                                                                                    xqName,
                                                                                                    table,
                                                                                                    teachingMethod,
                                                                                                    courseType,
                                                                                                    examinationMethod,
                                                                                                    id, kcbh, jczy010Id,
                                                                                                    szkclb, groupName,
                                                                                                    method))))))))))))))))))));
    }

    public static HNUSelectedCourse SelectFromCourse(HNUCourse course, SelectionMethod selectionMethod)
    {
        return new HNUSelectedCourse(course.SelectedStudentCount, course.TotalStudentCount, course.TotalLearningHours,
            course.TotalCredits, course.TeacherName, course.ClassName, course.CourseName, course.OfferInstitution,
            course.Campus, ((HNUCourseTimeTable)course.TimeTable).Clone(), course.TeachingMethod, course.CourseType, course.ExaminationMethod,
            course.Id, course.Code, course.Jczy010Id, course.ElectiveCourseType, course.GroupName, selectionMethod);
    }

    
    private static readonly List<PresentedData<HNUSelectedCourse>> PresentedData =
    [
        OfString("course.code", course => course.Code),
        OfString("course.name", course => course.CourseName),
        OfString("course.group_name", course => course.GroupName),
        OfString("course.class_code", course => course.ClassCode),
        OfString("course.selection_method", course => course.SelectionMethod.LocalizedName),
        OfString("course.teacher_name", course => course.TeacherName),
        OfString("course.class_name", course => course.ClassName),
        OfEnum("course.offer_institution", course => course.OfferInstitution),
        OfEnum("course.campus", course => course.Campus),
        new("course.total_learning_hours", course => course.TotalLearningHours),
        new("course.total_credit", course => course.TotalCredits),
        OfEnum("course.type", course => course.CourseType.Name),
        OfEnum("course.elective_type", course => course.ElectiveCourseType),
        OfEnum("course.teaching_method", course => course.TeachingMethod.ToString()),
        OfEnum("course.examination_method", course => course.ExaminationMethod.ToString()),
        OfString("course.student_count", course => $"{course.SelectedStudentCount}/{course.TotalStudentCount}"),
        OfString("course.time_table", course => string.Join("|", course.TimeTable.WeeklyInformation))
    ];
    
    private static readonly List<PresentedData<HNUSelectedCourse>> SimplePresentedData =
    [
        OfString("course.code", course => course.Code),
        OfString("course.name", course => course.CourseName),
        OfString("course.selection_method", course => course.SelectionMethod.LocalizedName),
        OfString("course.teacher_name", course => course.TeacherName),
        OfEnum("course.campus", course => course.Campus),
        OfEnum("course.type", course => course.CourseType.Name),
        OfEnum("course.elective_type", course => course.ElectiveCourseType),
    ];
    
    public new static List<PresentedData<HNUSelectedCourse>> GetPresentedData()
    {
        return PresentedData;
    }

    public new static List<PresentedData<HNUSelectedCourse>> GetSimplePresentedData()
    {
        return SimplePresentedData;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, {nameof(SelectionMethod)}: {SelectionMethod}";
    }
    
    internal static Result<SelectionMethod, HdjwError> SelectionMethodFromId(int id)
    {
        return id switch
        {
            0 => SelectionMethod.Preset,
            1 => SelectionMethod.SelfSelect,
            _ => new HdjwError($"Selection method id out of range, id: {id}")
        };
    }
}
