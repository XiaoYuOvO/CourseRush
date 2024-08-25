using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace CourseRush.Core.Util;

public abstract class Enum<T>(string name) where T : Enum<T>
{
    private static readonly List<T> MutableValues = [];
    private static readonly Dictionary<string, T> ValueByName = [];
    private static ImmutableList<T>? _values;
    private static FrozenDictionary<string, T>? _frozenValuesByName;
    public static ImmutableList<T> Values {
        get
        {
            if (_values == null) RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
            return _values ??= MutableValues.ToImmutableList();
        }
    }

    protected static T Register(T value)
    {
        MutableValues.Add(value);
        ValueByName[value.Name] = value;
        return value;
    }

    public static T ByName(string name)
    {
        if (_frozenValuesByName == null)
        {
            RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
            _frozenValuesByName = ValueByName.ToFrozenDictionary();
        }

        return _frozenValuesByName[name];
    }

    public string Name { get; } = name;
}