using System.Text.Json.Nodes;
using CourseRush.Core;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseCategory : ICourseCategory
{
    public string CategoryId { get; }
    public int CategoryNumber { get; }
    public string CategoryName { get; }
    public IReadOnlyList<ICourseSubcategory> SubCategories { get; }
    
    protected HNUCourseCategory(string categoryId, int categoryNumber, string categoryName, IReadOnlyList<ICourseSubcategory> subCategories)
    {
        CategoryId = categoryId;
        CategoryNumber = categoryNumber;
        CategoryName = categoryName;
        SubCategories = subCategories;
    }
    
    // protected HNUCourseCategory(string categoryId, int categoryNumber, string categoryName, List<HNUCourseSubcategory> subCategories) : this(categoryId, categoryNumber, categoryName, subCategories)
    // {
    // }

    public static Result<HNUCourseCategory, HdjwError> FromJson(JsonObject json)
    {
        var subcategories =
            json.Require("xkfl")
                .Bind(xkfl =>
                    (from jsonNode in xkfl.AsArray() where jsonNode != null
                        select HNUCourseSubcategory.FromJson(jsonNode.AsObject())).ToList()
                    .CombineResults())
                .GetOrDefault(new List<HNUCourseSubcategory>());
        return json.RequireString("id")
            .Bind(id => json.ParseInt("xklbbh")
                .Bind(xklbbh => json.RequireString("xklb_name")
                    .Bind<HNUCourseCategory>(xklbName => new HNUCourseCategory(id, xklbbh, xklbName, subcategories))));
    }
    
    public override string ToString()
    {
        return $"{nameof(CategoryId)}: {CategoryId}, {nameof(CategoryNumber)}: {CategoryNumber}, {nameof(CategoryName)}: {CategoryName}, \n   {nameof(SubCategories)}: {string.Join("\n   ",SubCategories)}";
    }
}

public class HNUCourseSubcategory : ICourseSubcategory
{
    private HNUCourseSubcategory(string id, string subcategoryName, int code)
    {
        Id = id;
        SubcategoryName = subcategoryName;
        Code = code;
    }

    public string Id { get; }
    public string SubcategoryName { get; }
    public int Code { get; }

    public static Result<HNUCourseSubcategory, HdjwError> FromJson(JsonObject json)
    {
        return json.RequireString("id")
            .Bind(id => json.RequireString("xkfl_name")
                .Bind(xkflName => json.ParseInt("xkflbh")
                    .Bind<HNUCourseSubcategory>(xkflbh => new HNUCourseSubcategory(id, xkflName, xkflbh))));
    }
}