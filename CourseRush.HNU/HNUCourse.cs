using System.Text.Json.Nodes;
using CourseRush.Core;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourse : Course
{
    public string Id { get; }
    public string Code { get; }

    public HNUCourse(int selectedStudentCount, int totalStudentCount, float totalLearningHours, float totalCredits, string teacherName, string className, string courseName, string offerInstitution, string campus, CourseTimeTable timeTable, TeachingMethod teachingMethod, CourseType courseType, ExaminationMethod examinationMethod, string id, string code) :
        base(selectedStudentCount, totalStudentCount, totalLearningHours, totalCredits, teacherName, className, courseName, offerInstitution, campus, timeTable, teachingMethod, courseType, examinationMethod)
    {
        Id = id;
        Code = code;
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
                                                                    .Bind<HNUCourse>(kcbh => new HNUCourse(xkrs, pkrs, zxs,zxf,sklsName,ktmcName,kcmcName,kkdwName,xqName,table, teachingMethod,courseType, examinationMethod, id, kcbh))))))))))))))));
    }

    public override string ToString()
    {
        return $"{base.ToString()}, {nameof(Id)}: {Id}, {nameof(Code)}: {Code}";
    }

    private static TeachingMethod IdToTeacherMethod(int id)
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

    private static ExaminationMethod IdToExaminationMethod(int id)
    {
        return id switch
        {
            0 => ExaminationMethod.Empty,
            1 => ExaminationMethod.Examination,
            2 => ExaminationMethod.Evaluation,
            _ => ExaminationMethod.Examination
        };
    }

    public override void AddCourseSelectionToJson(JsonObject jsonObject)
    {
        jsonObject["id"] = Id;
    }
}