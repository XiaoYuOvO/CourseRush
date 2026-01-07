using System.Text.Json.Nodes;
using CourseRush.Core;
using static CourseRush.HNU.HdjwJsonExtensions;
using HtmlAgilityPack;
using Resultful;
using static CourseRush.Core.PresentedData<CourseRush.HNU.HNUSelectedCourse>;

namespace CourseRush.HNU;

public class HNUSelectedCourse : HNUCourse, ISelectedCourse, IPresentedDataProvider<HNUSelectedCourse>, IJsonSerializable<HNUSelectedCourse, HdjwError>
{
    private HNUSelectedCourse(
        HNUCourseCategory? category,
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
        : base(category, selectedStudentCount, totalStudentCount, totalLearningHours, totalCredits, teacherName, className,"",
            courseName, offerInstitution, campus, timeTable, teachingMethod, courseType, examinationMethod, id, code,
            jczy010Id, electiveCourseType, groupName){
        SelectionMethod = selectionMethod;
    }

    private HNUSelectedCourse(float totalCredits,
        string teacherName,
        string courseName,
        string campus,
        CourseTimeTable timeTable,
        TeachingMethod teachingMethod,
        CourseType courseType,
        string id,
        string code,
        string groupName,
        SelectionMethod selectionMethod) : this(null, 0,0, 0.0f, totalCredits, teacherName, "",
        courseName, "", campus, timeTable, teachingMethod, courseType, ExaminationMethod.Empty, id, code,
        "", "", groupName, selectionMethod) { }

    public SelectionMethod SelectionMethod { get; }

    public new static Result<HNUSelectedCourse, HdjwError> FromJson(JsonObject node)
    {
        return HNUCourse.FromJson(node).Map(course => SelectFromCourse(course, SelectionMethod.Preset));
    }

    public static Result<HNUSelectedCourse, HdjwError> FromHtml(HtmlNode row)
    {
        return row.SelectSingleNode("td[1]//text()[normalize-space()]").ToOption().Map(node => node.InnerText.Trim()).Bind(courseCode => 
                row.SelectSingleNode("td[2]//text()[normalize-space()]").ToOption().Map(node => node.InnerText.Trim()).Bind(courseName => 
                    row.SelectSingleNode("td[3]//text()").ToOption().Map(node => node.InnerText.Trim()).Bind(courseNote =>
                        row.SelectSingleNode("td[4]//text()[normalize-space()]").ToOption().Map(node => node.InnerText.Trim()).Bind(teachingMethod => 
                            row.SelectSingleNode("td[5]//text()[normalize-space()]").ToOption().Map(node => node.InnerText.Trim()).Bind(totalCredit =>
                                row.SelectSingleNode("td[6]//text()[normalize-space()]").ToOption().Map(node => node.InnerText.Trim()).Bind(courseType =>
                                    row.SelectSingleNode("td[7]//text()[normalize-space()]").ToOption().Map(node => node.InnerText.Trim()).Bind(teacher => 
                                        row.SelectSingleNode("td[9]//text()[normalize-space()]").ToOption().Map(node => node.InnerText.Trim()).Bind(campus => 
                                            HNUCourseTimeTable.FromHtmlNode(row.SelectNodes("td[8]//text()[normalize-space()]"), teacher, campus).ReturnOrValue(_ => HNUCourseTimeTable.Empty).ToOption().Bind(timeTable =>
                                                row.SelectSingleNode("td[10]//text()[normalize-space()]").ToOption().Map(node => node.InnerText.Trim()).Bind(selectionMethod =>
                                                    row.SelectSingleNode("td[11]//div[@class='xkButton']").ToOption()
                                                        .Map(button => button.Id.Replace("div_", ""))
                                                        .Bind<Result<HNUSelectedCourse, HdjwError>>(courseId =>
                                                            totalCredit.Parse<float>()
                                                                .Bind<HNUSelectedCourse>(totalCreditFloat =>
                                                                    new HNUSelectedCourse(totalCreditFloat, teacher,
                                                                        courseName, campus, timeTable,
                                                                        TeachingMethod.ClassTeaching,
                                                                        HNUCourseType.TypeFromName(courseType),
                                                                        courseId,
                                                                        courseCode, courseNote,
                                                                        SelectionMethod.Preset)))))))))))))
            .Or(HdjwError.Create("Cannot read selected course"));
    }

    public static HNUSelectedCourse SelectFromCourse(HNUCourse course, SelectionMethod selectionMethod)
    {
        return new HNUSelectedCourse(course.Category, course.SelectedStudentCount, course.TotalStudentCount, course.TotalLearningHours,
            course.TotalCredits, course.TeacherName, course.ClassName, course.CourseName, course.OfferInstitution,
            course.Campus, ((HNUCourseTimeTable)course.TimeTable).Clone(), course.TeachingMethod, course.CourseType, course.ExaminationMethod,
            course.Id, course.Code, course.Jx02Id, course.ElectiveCourseType, course.GroupName, selectionMethod);
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
