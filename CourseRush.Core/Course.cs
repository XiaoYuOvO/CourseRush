namespace CourseRush.Core;

public class Course
{
    public Course(int selectedStudentCount, int totalStudentCount, float totalLearningHours, float totalCredits, string teacherName, string className, string lessonName, string offerInstitution, string campus, CourseTimeTable timeTable, TeachingMethod teachingMethod, CommonLessonType lessonType)
    {
        SelectedStudentCount = selectedStudentCount;
        TotalStudentCount = totalStudentCount;
        TotalLearningHours = totalLearningHours;
        TotalCredits = totalCredits;
        TeacherName = teacherName;
        ClassName = className;
        LessonName = lessonName;
        OfferInstitution = offerInstitution;
        Campus = campus;
        TimeTable = timeTable;
        TeachingMethod = teachingMethod;
        LessonType = lessonType;
    }

    public int SelectedStudentCount { get; }
    public int TotalStudentCount { get; }

    public float TotalLearningHours { get; }
    public float TotalCredits { get; }
    public string TeacherName { get; }
    public string ClassName { get; }
    public string LessonName { get; }
    public string OfferInstitution { get; }
    public string Campus { get; }

    public CourseTimeTable TimeTable { get; }
    public TeachingMethod TeachingMethod { get; }
    public CommonLessonType LessonType { get; }
}