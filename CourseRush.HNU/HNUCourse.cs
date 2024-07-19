using System.Text.Json.Nodes;
using CourseRush.Core;

namespace CourseRush.HNU;

public class HNUCourse : Course
{
    private readonly string _jczy013Id;
    private readonly string _xkgl017Id;
    private readonly string _xkgl019Id;
    private readonly string _id;
    public HNUCourse(int selectedStudentCount, int totalStudentCount, float totalLearningHours, float totalCredits, string teacherName, string className, string lessonName, string offerInstitution, string campus, CourseTimeTable timeTable, TeachingMethod teachingMethod, CommonLessonType lessonType, string jczy013Id, string xkgl017Id, string xkgl019Id, string id) :
        base(selectedStudentCount, totalStudentCount, totalLearningHours, totalCredits, teacherName, className, lessonName, offerInstitution, campus, timeTable, teachingMethod, lessonType)
    {
        _jczy013Id = jczy013Id;
        _xkgl017Id = xkgl017Id;
        _xkgl019Id = xkgl019Id;
        _id = id;
    }

    public static HNUCourse FromJson(JsonObject jsonObject)
    {
        return null;
    }
}