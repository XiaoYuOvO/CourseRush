using System.Text.Json.Nodes;
using CourseRush.Core;
using Resultful;

namespace CourseRush.HNU;

public class HNUSelectedCourse : HNUCourse, ISelectedCourse
{
    public SelectionMethod SelectionMethod { get; }

    public HNUSelectedCourse(int selectedStudentCount, int totalStudentCount, float totalLearningHours, float totalCredits, string teacherName, string className, string courseName, string offerInstitution, string campus, CourseTimeTable timeTable, TeachingMethod teachingMethod, CourseType courseType, ExaminationMethod examinationMethod, string id, string code, string jczy010Id, string electiveCourseType, SelectionMethod selectionMethod) : 
        base(selectedStudentCount, totalStudentCount, totalLearningHours, totalCredits, teacherName, className, courseName, offerInstitution, campus, timeTable, teachingMethod, courseType, examinationMethod, id, code, jczy010Id, electiveCourseType)
    {
        SelectionMethod = selectionMethod;
    }

    public new static Result<HNUSelectedCourse, HdjwError> FromJson(JsonObject jsonObject)
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
                                                                            .Bind(szkclb => jsonObject.ParseInt("xkfscode").Bind(FromId)
                                                                                .Bind<HNUSelectedCourse>(method => new HNUSelectedCourse(xkrs, pkrs, zxs,zxf,sklsName,ktmcName,kcmcName,kkdwName,xqName,table, teachingMethod,courseType, examinationMethod, id, kcbh, jczy010Id, szkclb, method)))))))))))))))))));
    }

    public override string ToString()
    {
        return $"{base.ToString()}, {nameof(SelectionMethod)}: {SelectionMethod}";
    }
    
    internal static Result<SelectionMethod, HdjwError> FromId(int id)
    {
        return id switch
        {
            0 => SelectionMethod.Preset,
            1 => SelectionMethod.SelfSelection,
            _ => new HdjwError($"Selection method id out of range, id: {id}")
        };
    }
}
