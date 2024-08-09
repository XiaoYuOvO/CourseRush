namespace CourseRush.Core;

public interface ISearchType
{
}

public class SearchType<TValue, TTarget> : ISearchType
{
    private readonly Func<TValue, TTarget> _extractor;
    public string DataTip { get; }

    public SearchType(Func<TValue, TTarget> extractor, string dataTip)
    {
        _extractor = extractor;
        DataTip = dataTip;
    }

    public TTarget Extract(TValue course)
    {
        return _extractor(course);
    }
}

public class EnumSearchType<TValue> : SearchType<TValue, string>
{
    public EnumSearchType(Func<TValue, string> extractor, string dataTip) : base(extractor, dataTip)
    {
    }
}