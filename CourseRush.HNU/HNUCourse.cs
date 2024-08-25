using System.Text.Json.Nodes;
using CourseRush.Core;
using Resultful;
using static CourseRush.Core.PresentedData<CourseRush.HNU.HNUCourse>;

namespace CourseRush.HNU;

public class HNUCourse(
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
    string groupName)
    : Course<HNUCourse>(selectedStudentCount, totalStudentCount, totalLearningHours, totalCredits, teacherName,
            className, courseName, offerInstitution, campus, timeTable, teachingMethod, courseType, examinationMethod),
        IPresentedDataProvider<HNUCourse>, IJsonSerializable<HNUCourse, HdjwError>
{
    public string Id { get; } = id;
    public string Code { get; } = code;
    public string Jczy010Id { get; } = jczy010Id;
    public string ElectiveCourseType { get; } = electiveCourseType;
    public string GroupName { get; } = groupName;

    public static Result<HNUCourse, HdjwError> FromJson(JsonObject jsonObject)
    {
        return jsonObject.RequireInt("xkrs")
            .Bind(xkrs => jsonObject.RequireInt("pkrs")
                .Bind(pkrs => jsonObject.RequireFloat("zxs")
                    .Bind(zxs => jsonObject.RequireFloat("zxf")
                        .Bind(zxf => jsonObject.GetString("skls_name","无")
                            .Bind(sklsName => jsonObject.RequireString("ktmc_name")
                                .Bind(ktmcName => jsonObject.RequireString("kcmc_name")
                                    .Bind(kcmcName => jsonObject.RequireString("kkdw_name")
                                        .Bind(kkdwName => jsonObject.RequireString("xq_name")
                                            .Bind(xqName => jsonObject.GetString("kbinfo", "").Bind(HNUCourseTimeTable.FromString)
                                                .Bind(table => jsonObject.ParseInt("skfscode").Map(IdToTeacherMethod)
                                                    .Bind(teachingMethod => jsonObject.ParseInt("kclbcode").Map(HNUCourseType.TypeFromCode)
                                                        .Bind(courseType => jsonObject.ParseInt("khfscode",0).Map(IdToExaminationMethod)
                                                            .Bind(examinationMethod => jsonObject.RequireString("id")
                                                                .Bind(id => jsonObject.RequireString("kcbh")
                                                                    .Bind(kcbh => jsonObject.RequireString("jczy010id")
                                                                        .Bind(jczy010Id => jsonObject.GetString("szkclb_name", "无")
                                                                            .Bind(szkclb => jsonObject.GetString("fzmc_name", "")
                                                                                .Bind<HNUCourse>(additionalName => new HNUCourse(xkrs, pkrs, zxs,zxf,sklsName,ktmcName,kcmcName,kkdwName,xqName,table, teachingMethod,courseType, examinationMethod, id, kcbh, jczy010Id, szkclb, additionalName)))))))))))))))))));
    }

    public JsonObject ToJson()
    {
        return new JsonObject
        {
            ["xkrs"] = SelectedStudentCount,
            ["pkrs"] = TotalStudentCount,
            ["zxs"] = TotalLearningHours,
            ["zxf"] = TotalCredits,
            ["skls_name"] = TeacherName,
            ["ktmc_name"] = ClassName,
            ["kcmc_name"] = CourseName,
            ["kkdw_name"] = OfferInstitution,
            ["xq_name"] = Campus,
            ["kbinfo"] = TimeTable.ToJsonString(),
            ["skfscode"] = TeacherMethodToId(TeachingMethod).ToString(),
            ["kclbcode"] = CourseType.Code.ToString(),
            ["khfscode"] = ExaminationMethodToId(ExaminationMethod).ToString(),
            ["id"] = Id,
            ["kcbh"] = Code,
            ["jczy010id"] = Jczy010Id,
            ["szkclb_name"] = ElectiveCourseType,
            ["fzmc_name"] = GroupName
        };
    }

    public override string ToSimpleString()
    {
        return $"[{Code}]{CourseName}-{TeacherName}";
    }

    public override string ToString()
    {
        return $"{base.ToString()}, {nameof(Id)}: {Id}, {nameof(Code)}: {Code}, {nameof(Jczy010Id)}: {Jczy010Id}, {nameof(ElectiveCourseType)} : {ElectiveCourseType}";
    }

    protected static TeachingMethod IdToTeacherMethod(int id)
    {
        return id switch
        {
            0 => TeachingMethod.ClassTeaching,
            1 => TeachingMethod.Practice,
            2 => TeachingMethod.CourseInstruction,
            4 => TeachingMethod.PhysicsEducation,
            6 => TeachingMethod.JointPractice,
            _ => TeachingMethod.ClassTeaching,
        };
    }
    
    protected static int TeacherMethodToId(TeachingMethod method)
    {
         if(method == TeachingMethod.ClassTeaching) return 0;
         if(method == TeachingMethod.Practice) return 1;
         if(method == TeachingMethod.CourseInstruction) return 2;
         if(method == TeachingMethod.PhysicsEducation) return 4;
         if(method == TeachingMethod.JointPractice) return 6;
         return 0;
    }

    protected static ExaminationMethod IdToExaminationMethod(int id)
    {
        return id switch
        {
            0 => ExaminationMethod.Empty,
            1 => ExaminationMethod.Examination,
            2 => ExaminationMethod.Evaluation,
            _ => ExaminationMethod.Examination
        };
    }
    
    protected static int ExaminationMethodToId(ExaminationMethod method)
    {
        if(method == ExaminationMethod.Empty) return 0; 
        if(method == ExaminationMethod.Examination) return 1; 
        if(method == ExaminationMethod.Evaluation) return 2; 
        return 1; 
    }

    public override void AddCourseSelectionToJson(JsonObject jsonObject)
    {
        jsonObject["id"] = Id;
    }

    protected bool Equals(HNUCourse other)
    {
        return Id == other.Id && Jczy010Id == other.Jczy010Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((HNUCourse)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Jczy010Id);
    }

    private static readonly List<PresentedData<HNUCourse>> PresentedData =
    [
        OfString("course.code", course => course.Code),
        OfString("course.name", course => course.CourseName),
        OfString("course.group_name", course => course.GroupName),
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
    
    public static List<PresentedData<HNUCourse>> GetPresentedData()
    {
        return PresentedData;
    }

    private static readonly List<PresentedData<HNUCourse>> SimplePresentedData =
    [
        OfString("course.code", course => course.Code),
        OfString("course.name", course => course.CourseName),
        OfString("course.teacher_name", course => course.TeacherName),
        OfEnum("course.campus", course => course.Campus),
        OfEnum("course.type", course => course.CourseType.Name),
        OfEnum("course.elective_type", course => course.ElectiveCourseType),
    ];
    public static List<PresentedData<HNUCourse>> GetSimplePresentedData()
    {
        return SimplePresentedData;
    }
}