using System.Text.Json.Nodes;
using CourseRush.Core;
using Resultful;
using static CourseRush.Core.PresentedData<CourseRush.HNU.HNUCourse>;

namespace CourseRush.HNU;

public class HNUCourse : Course<HNUCourse>
{
    public static readonly Codec<HNUCourse, HdjwError> Codec = new(ToJson, FromJson, HdjwError.Combine);
    public string Id { get; }
    public string Code { get; }
    public string Jczy010Id { get; }
    public string ElectiveCourseType { get; }

    public HNUCourse(int selectedStudentCount, int totalStudentCount, float totalLearningHours, float totalCredits, string teacherName, string className, string courseName, string offerInstitution, string campus, CourseTimeTable timeTable, TeachingMethod teachingMethod, CourseType courseType, ExaminationMethod examinationMethod, string id, string code, string jczy010Id, string electiveCourseType) :
        base(selectedStudentCount, totalStudentCount, totalLearningHours, totalCredits, teacherName, className, courseName, offerInstitution, campus, timeTable, teachingMethod, courseType, examinationMethod)
    {
        Id = id;
        Code = code;
        Jczy010Id = jczy010Id;
        ElectiveCourseType = electiveCourseType;
    }

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
                                                                            .Bind<HNUCourse>(szkclb => new HNUCourse(xkrs, pkrs, zxs,zxf,sklsName,ktmcName,kcmcName,kkdwName,xqName,table, teachingMethod,courseType, examinationMethod, id, kcbh, jczy010Id, szkclb))))))))))))))))));
    }

    public static JsonObject ToJson(HNUCourse course)
    {
        return new JsonObject
        {
            ["xkrs"] = course.SelectedStudentCount,
            ["pkrs"] = course.TotalStudentCount,
            ["zxs"] = course.TotalLearningHours,
            ["zxf"] = course.TotalCredits,
            ["skls_name"] = course.TeacherName,
            ["ktmc_name"] = course.ClassName,
            ["kkdw_name"] = course.OfferInstitution,
            ["xq_name"] = course.Campus,
            ["kbinfo"] = course.TimeTable.ToJsonString(),
            ["skfscode"] = TeacherMethodToId(course.TeachingMethod).ToString(),
            ["kclbcode"] = course.CourseType.Code.ToString(),
            ["khfscode"] = ExaminationMethodToId(course.ExaminationMethod).ToString(),
            ["id"] = course.Id,
            ["kcbh"] = course.Code,
            ["jczy010id"] = course.Jczy010Id,
            ["szkclb_name"] = course.ElectiveCourseType
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

    public static List<PresentedData<HNUCourse>> BuildPresentedData()
    {
        return new List<PresentedData<HNUCourse>>
        {
            OfString("course.code", course => course.Code),
            OfString("course.name", course => course.CourseName),
            OfString("course.teacher_name", course => course.TeacherName),
            OfString("course.class_name", course => course.ClassName),
            OfEnum("course.offer_institution", course => course.OfferInstitution),
            OfEnum("course.campus", course => course.Campus),
            new("course.total_learning_hours", course => course.TotalLearningHours),
            new("course.total_credit", course => course.TotalCredits),
            OfEnum("course.type", course => course.CourseType.Name),
            OfEnum("course.teaching_method", course => course.TeachingMethod.ToString()),
            OfEnum("course.examination_method", course => course.ExaminationMethod.ToString()),
            OfString("course.student_count", course => $"{course.SelectedStudentCount}/{course.TotalStudentCount}"),
            OfString("course.time_table", course => string.Join("|", course.TimeTable.WeeklyInformation))
        };
    }
}