using System.Text.Json.Nodes;
using CourseRush.Core;
using CourseRush.Core.Util;
using Resultful;

namespace CourseRush.HNU;

public class HNUCourseCategory(string categoryId, string categoryName, string categoryOperator, string categoryQuery)
    : ICourseCategory
{
    public string CategoryId { get; } = categoryId;
    public string CategoryOperator { get; } = categoryOperator;
    public string CategoryQuery { get; } = categoryQuery;
    public string CategoryName { get; } = categoryName;

    // protected HNUCourseCategory(string categoryId, int categoryNumber, string categoryName, List<HNUCourseSubcategory> subCategories): this(categoryId, categoryNumber, categoryName, subCategories)
    // {
    // }

    public static Result<HNUCourseCategory, HdjwError> FromJson(JsonObject json)
    {
        //TODO Add query and operation
        var subcategories =
            json.Require("xkfl")
                .Bind(xkfl =>
                    (from jsonNode in xkfl.AsArray() where jsonNode != null
                        select HNUCourseSubcategory.FromJson(jsonNode.AsObject())).ToList()
                    .CombineResults())
                .GetOrDefault(new List<HNUCourseSubcategory>());
        return json.RequireString("id")
            .Bind<HNUCourseCategory>(id => json.ParseInt("xklbbh")
                .Bind<HNUCourseCategory>(xklbbh => json.RequireString("xklb_name")
                    .Bind<HNUCourseCategory>(xklbName => new HNUCourseCategory(id, xklbName, id, id))));
    }
    
    public override string ToString()
    {
        return $"{nameof(CategoryId)}: {CategoryId}, {nameof(CategoryName)}: {CategoryName}";
    }
}

public class HNUCourseSubcategory : ICourseSubcategory
{
    private HNUCourseSubcategory(string id, string subcategoryName, int code, string xkgl004Id)
    {
        Id = id;
        SubcategoryName = subcategoryName;
        Code = code;
        Xkgl004id = xkgl004Id;
    }

    public string Id { get; }
    public string SubcategoryName { get; }
    public string Xkgl004id { get; }
    public int Code { get; }

    public static Result<HNUCourseSubcategory, HdjwError> FromJson(JsonObject json)
    {
        return json.RequireString("id")
            .Bind(id => json.RequireString("xkfl_name")
                .Bind(xkflName => json.ParseInt("xkflbh")
                    .Bind<HNUCourseSubcategory>(xkflbh => json.RequireString("xkgl004id")
                        .Bind<HNUCourseSubcategory>(xkgl004id => new HNUCourseSubcategory(id, xkflName, xkflbh, xkgl004id)))));
    }
}