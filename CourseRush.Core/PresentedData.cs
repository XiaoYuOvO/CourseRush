using System.Globalization;

namespace CourseRush.Core;

public class PresentedData<TValue>
{
    public string DataTip { get; }
    private readonly Func<TValue, string> _getter;
    public ISearchType SearchType { get; }

    private PresentedData(string dataTipKey, Func<TValue, string> getter, Func<string, ISearchType> searchTypeFactory)
    {
        DataTip = Language.ResourceManager.GetString(dataTipKey, CultureInfo.CurrentCulture) ?? throw new MissingMemberException($"Cannot find {dataTipKey} in language dictionary");
        _getter = getter;
        SearchType = searchTypeFactory(DataTip);
    }

    public static PresentedData<TValue> OfString(string dataTipKey, Func<TValue, string> getter)
    {
        return new PresentedData<TValue>(dataTipKey, getter, dataTip => new SearchType<TValue, string>(getter, dataTip));
    }
    
    public static PresentedData<TValue> OfEnum(string dataTipKey, Func<TValue, string> getter)
    {
        return new PresentedData<TValue>(dataTipKey, getter, dataTip => new EnumSearchType<TValue>(getter, dataTip));
    }

    public PresentedData(string dataTipKey, Func<TValue, float> getter)
    {
        DataTip = Language.ResourceManager.GetString(dataTipKey, CultureInfo.CurrentCulture) ?? throw new MissingMemberException($"Cannot find {dataTipKey} in language dictionary");
        var valueKey = dataTipKey + ".value";
        _getter = course => string.Format(Language.ResourceManager.GetString(valueKey, CultureInfo.CurrentCulture) ?? throw new MissingMemberException($"Cannot find {valueKey} in language dictionary"), getter(course));
        SearchType = new SearchType<TValue, float>(getter, DataTip);
    }
    
    public PresentedData(string dataTipKey, Func<TValue, int> getter)
    {
        DataTip = Language.ResourceManager.GetString(dataTipKey, CultureInfo.CurrentCulture) ?? throw new MissingMemberException($"Cannot find {dataTipKey} in language dictionary");
        var valueKey = dataTipKey + ".value";
        _getter = course => string.Format(Language.ResourceManager.GetString(valueKey,CultureInfo.CurrentCulture) ?? throw new MissingMemberException($"Cannot find {valueKey} in language dictionary"), getter(course));
        SearchType = new SearchType<TValue, int>(getter, DataTip);
    }

    public string? GetValue(TValue course)
    {
        return _getter(course);
    }
}