namespace CourseRush.Core;

public interface ICourseCategory
{


    public string CategoryId { get; }
    public string CategoryName { get; }
    public IReadOnlyList<ICourseSubcategory> SubCategories { get; }
    
 
}