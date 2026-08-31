using System.Resources;

namespace CourseRush.Core.Util;

public record TranslatableText(string TranslationKey, params object[] TranslationParams)
{
    public string Translate(ResourceManager manager)
    {
        var val = manager.GetString(TranslationKey);
        return val == null ? string.Format(TranslationKey, TranslationParams) : string.Format(val, TranslationParams);
    }
    
    public string Translate(ResourceManager manager, object? parameter)
    {
        var val = manager.GetString(TranslationKey);
        return val == null ? string.Format(TranslationKey, TranslationParams, parameter) : string.Format(val, TranslationParams, parameter);
    }

    public static TranslatableText Of(string translationKey, params object[] translationParams)
    {
        return new TranslatableText(translationKey, translationParams);
    }
    

    public static implicit operator TranslatableText(string translationKey)
    {
        return new TranslatableText(translationKey);
    }
}