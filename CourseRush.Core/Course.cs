using System.Text.Json.Nodes;

namespace CourseRush.Core;

public interface ICourse
{
    public int SelectedStudentCount { get; }
    public int TotalStudentCount { get; }
    public float TotalLearningHours { get; }
    public float TotalCredits { get; }
    public string TeacherName { get; }
    public string ClassName { get; }
    public string CourseName { get; }
    public string OfferInstitution { get; }
    public string Campus { get; }

    public CourseTimeTable TimeTable { get; }
    public TeachingMethod TeachingMethod { get; }
    public CourseType CourseType { get; }
    public ExaminationMethod ExaminationMethod { get; }
    public Dictionary<ICourse, IEnumerable<CourseWeeklyTime.ConflictResult>> ConflictsCache { get; }

    public string ToSimpleString();
}

public abstract class Course<TCourse> : ICourse where TCourse : ICourse
{
    protected Course(int selectedStudentCount, int totalStudentCount, float totalLearningHours, float totalCredits, string teacherName, string className, string courseName, string offerInstitution, string campus, CourseTimeTable timeTable, TeachingMethod teachingMethod, CourseType courseType, ExaminationMethod examinationMethod)
    {
        SelectedStudentCount = selectedStudentCount;
        TotalStudentCount = totalStudentCount;
        TotalLearningHours = totalLearningHours;
        TotalCredits = totalCredits;
        TeacherName = teacherName;
        ClassName = className;
        CourseName = courseName;
        OfferInstitution = offerInstitution;
        Campus = campus;
        TimeTable = timeTable;
        TeachingMethod = teachingMethod;
        CourseType = courseType;
        ExaminationMethod = examinationMethod;
        timeTable.WeeklyInformation.ForEach(time => time.BindingCourse = this);
    }

    public int SelectedStudentCount { get; }
    public int TotalStudentCount { get; }

    public float TotalLearningHours { get; }
    public float TotalCredits { get; }
    public string TeacherName { get; }
    public string ClassName { get; }
    public string CourseName { get; }
    public string OfferInstitution { get; }
    public string Campus { get; }

    public CourseTimeTable TimeTable { get; }
    public TeachingMethod TeachingMethod { get; }
    public CourseType CourseType { get; }
    public ExaminationMethod ExaminationMethod { get; }

    public Dictionary<ICourse, IEnumerable<CourseWeeklyTime.ConflictResult>> ConflictsCache { get; } = new();
    public abstract string ToSimpleString();

    public override string ToString()
    {
        return $"{nameof(SelectedStudentCount)}: {SelectedStudentCount}, {nameof(TotalStudentCount)}: {TotalStudentCount}, {nameof(TotalLearningHours)}: {TotalLearningHours}, {nameof(TotalCredits)}: {TotalCredits}, {nameof(TeacherName)}: {TeacherName}, {nameof(ClassName)}: {ClassName}, {nameof(CourseName)}: {CourseName}, {nameof(OfferInstitution)}: {OfferInstitution}, {nameof(Campus)}: {Campus}, {nameof(TimeTable)}: {TimeTable}, {nameof(TeachingMethod)}: {TeachingMethod}, {nameof(CourseType)}: {CourseType}, {nameof(ExaminationMethod)}: {ExaminationMethod}";
    }

    public abstract void AddCourseSelectionToJson(JsonObject jsonObject);

    public abstract override bool Equals(object? obj);
    
    public abstract override int GetHashCode();
}