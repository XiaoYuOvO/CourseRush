using System.Text.Json.Nodes;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class CourseCategory
{
    private CourseCategory(string categoryId, int categoryNumber, string categoryName, List<CourseSubcategory> subCategories)
    {
        CategoryId = categoryId;
        CategoryNumber = categoryNumber;
        CategoryName = categoryName;
        SubCategories = subCategories;
    }

    public string CategoryId { get; }
    public int CategoryNumber { get; }
    public string CategoryName { get; }
    public List<CourseSubcategory> SubCategories { get; }

    public static Result<CourseCategory, HdjwError> FromJson(JsonObject json)
    {
        var subcategories =
            json.Require("xkfl")
                .Bind(xkfl =>
                    (from jsonNode in xkfl.AsArray() where jsonNode != null
                        select CourseSubcategory.FromJson(jsonNode.AsObject())).ToList()
                    .CombineResults(HdjwError.Combine))
                .GetOrDefault(new List<CourseSubcategory>());
        return json.RequireString("id")
            .Bind(id => json.ParseInt("xklbbh")
                .Bind(xklbbh => json.RequireString("xklb_name")
                    .Bind<CourseCategory>(xklbName => new CourseCategory(id, xklbbh, xklbName, subcategories))));
    }

    public override string ToString()
    {
        return $"{nameof(CategoryId)}: {CategoryId}, {nameof(CategoryNumber)}: {CategoryNumber}, {nameof(CategoryName)}: {CategoryName}, \n   {nameof(SubCategories)}: {string.Join("\n   ",SubCategories)}";
    }
}

public class CourseSubcategory
{
    private CourseSubcategory(string id, string subcategoryName, int code)
    {
        Id = id;
        SubcategoryName = subcategoryName;
        Code = code;
    }

    public string Id { get; }
    public string SubcategoryName { get; }
    public int Code { get; }

    public static Result<CourseSubcategory, HdjwError> FromJson(JsonObject json)
    {
        return json.RequireString("id")
            .Bind(id => json.RequireString("xkfl_name")
                .Bind(xkflName => json.ParseInt("xkflbh")
                    .Bind<CourseSubcategory>(xkflbh => new CourseSubcategory(id, xkflName, xkflbh))));
    }
}