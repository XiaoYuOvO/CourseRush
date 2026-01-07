using System.Globalization;
using System.Text.Json.Nodes;
using CourseRush.Core;
using Resultful;
using static CourseRush.Core.PresentedData<CourseRush.HNU.HNUCourse>;

namespace CourseRush.HNU;

public class HNUCourse(
    HNUCourseCategory? category,
    int selectedStudentCount,
    int totalStudentCount,
    float totalLearningHours,
    float totalCredits,
    string teacherName,
    string className,
    string classCode,
    string courseName,
    string offerInstitution,
    string campus,
    CourseTimeTable timeTable,
    TeachingMethod teachingMethod,
    CourseType courseType,
    ExaminationMethod examinationMethod,
    string id,
    string code,
    string jx02Id,
    string electiveCourseType,
    string groupName)
    : Course<HNUCourseCategory>(category, selectedStudentCount, totalStudentCount, totalLearningHours, totalCredits, teacherName,
            className, courseName, offerInstitution, campus, timeTable, teachingMethod, courseType, examinationMethod),
        IPresentedDataProvider<HNUCourse>, IJsonSerializable<HNUCourse, HdjwError>
{
    public string Id { get; } = id;
    public string Code { get; } = code;
    public string Jx02Id { get; } = jx02Id;
    public string ElectiveCourseType { get; } = electiveCourseType;
    public string GroupName { get; } = groupName;
    public string ClassCode { get; } = classCode;

    public static Result<HNUCourse, HdjwError> FromJson(JsonObject jsonObject, HNUCourseCategory? category = null)
    {
        return jsonObject.GetString("jx0404id", "")
            .Bind<HNUCourse>(jx0404id => jsonObject.GetInt("xkrs", 0)
                .Bind<HNUCourse>(xkrs => jsonObject.GetInt("pkrs", 0)
                    .Bind<HNUCourse>(pkrs => jsonObject.RequireFloat("zxs")
                        .Bind<HNUCourse>(zxs => jsonObject.RequireFloat("xf")
                            .Bind<HNUCourse>(xf => jsonObject.GetString("skls", "无")
                                .Bind<HNUCourse>(sklsName => jsonObject.GetString("ktmc", "无")
                                    .Bind<HNUCourse>(ktmcName => jsonObject.RequireString("kcmc")
                                        .Bind<HNUCourse>(kcmcName => jsonObject.RequireString("dwmc")
                                            .Bind<HNUCourse>(kkdwName => jsonObject.RequireString("xqmc")
                                                .Bind<HNUCourse>(xqName => 
                                                {
                                                    var table = jsonObject.RequireArray("kkapList")
                                                        .Bind(HNUCourseTimeTable.FromJson)
                                                        .ReturnOrValue(_ => HNUCourseTimeTable.Empty);
                                                        return jsonObject.ParseInt("skfs", "0")
                                                            .Map(IdToTeacherMethod)
                                                            .Bind<HNUCourse>(teachingMethod => jsonObject
                                                                .ParseInt("kcxzm")
                                                                .Map(HNUCourseType.TypeFromCode) //TODO Adapt new code
                                                                .Bind<HNUCourse>(courseType => jsonObject
                                                                    .ParseInt("khfscode", 0)
                                                                    .Map(IdToExaminationMethod)
                                                                    .Bind<HNUCourse>(examinationMethod => jsonObject
                                                                        .GetString("kxh", "")
                                                                        .Bind<HNUCourse>(id => jsonObject
                                                                            .RequireString("kch")
                                                                            .Bind<HNUCourse>(kcbh => jsonObject
                                                                                .RequireString("jx02id")
                                                                                .Bind<HNUCourse>(jx02id => jsonObject
                                                                                    .GetString("tsTskflMc", "无")
                                                                                    .Bind<HNUCourse>(szkclb =>
                                                                                        jsonObject
                                                                                            .GetString("fzmc", "")
                                                                                            .Bind<
                                                                                                HNUCourse>(additionalName =>
                                                                                                new HNUCourse(category,
                                                                                                    xkrs, pkrs, zxs,
                                                                                                    xf, sklsName,
                                                                                                    ktmcName,
                                                                                                    jx0404id, kcmcName,
                                                                                                    kkdwName,
                                                                                                    xqName, table,
                                                                                                    teachingMethod,
                                                                                                    courseType,
                                                                                                    examinationMethod,
                                                                                                    id, kcbh,
                                                                                                    jx02id, szkclb,
                                                                                                    additionalName)))))))));
                                                    }))))))))));
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
            ["jczy010id"] = Jx02Id,
            ["szkclb_name"] = ElectiveCourseType,
            ["fzmc_name"] = GroupName
        };
    }

    public static Result<HNUCourse, HdjwError> FromJson(JsonObject jsonObject) => FromJson(jsonObject, null);

    public override string ToSimpleString()
    {
        return $"[{Code}]{CourseName}-{TeacherName}";
    }

    public override string ToString()
    {
        return $"{base.ToString()}, {nameof(Id)}: {Id}, {nameof(Code)}: {Code}, {nameof(Jx02Id)}: {Jx02Id}, {nameof(ElectiveCourseType)} : {ElectiveCourseType}";
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
        jsonObject["jczy010id"] = Jx02Id;
        jsonObject["xkfsid"] = "1";
        jsonObject["xkfs_name"] = "一选";
        jsonObject["zxf"] = TotalCredits.ToString(CultureInfo.InvariantCulture);
        jsonObject["zxs"] = TotalLearningHours.ToString(CultureInfo.InvariantCulture);
        jsonObject["isTqxd"] = "0";
        jsonObject["kcbh"] = Code;
        jsonObject["falb"] = "1";
        jsonObject["khfs"] = ExaminationMethodToId(ExaminationMethod).ToString(CultureInfo.InvariantCulture);
    }

    public override void AddCourseSelectionToWebForms(Dictionary<string, string> webForms)
    {
        webForms.Add("kcid", Jx02Id);
        webForms.Add("cfbs", "null");
        webForms.Add("jx0404id", ClassCode);
        webForms.Add("xkzy", "");
        webForms.Add("trjf", "");
    }

    protected bool Equals(HNUCourse other)
    {
        return Id == other.Id && Jx02Id == other.Jx02Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((HNUCourse)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Jx02Id);
    }

    private static readonly List<PresentedData<HNUCourse>> PresentedData =
    [
        OfString("course.code", course => course.Code),
        OfString("course.name", course => course.CourseName),
        OfString("course.group_name", course => course.GroupName),
        OfString("course.teacher_name", course => course.TeacherName),
        new("course.total_learning_hours", course => course.TotalLearningHours),
        new("course.total_credit", course => course.TotalCredits),
        OfString("course.student_count", course => $"{course.SelectedStudentCount}/{course.TotalStudentCount}"),
        OfEnum("course.campus", course => course.Campus),
        OfEnum("course.offer_institution", course => course.OfferInstitution),
        OfEnum("course.type", course => course.CourseType.Name),
        OfEnum("course.elective_type", course => course.ElectiveCourseType),
        OfEnum("course.teaching_method", course => course.TeachingMethod.ToString()),
        OfEnum("course.examination_method", course => course.ExaminationMethod.ToString()),
        OfString("course.time_table", course => string.Join("|", course.TimeTable.WeeklyInformation)),
        OfString("course.class_code", course => course.ClassCode),
        OfString("course.class_name", course => course.ClassName),
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