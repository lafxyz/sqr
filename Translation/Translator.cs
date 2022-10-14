using Newtonsoft.Json.Linq;
using QuickType;
using Serilog;

namespace SQR.Translation;

public class Translator
{
    public Dictionary<LanguageCode, Language.Language> Languages => _languages;
    public Dictionary<string, LanguageCode> LocaleMap => _localeMap;
    
    private Dictionary<string, LanguageCode> _localeMap = new();
    private Dictionary<LanguageCode, Language.Language> _languages = new();

    public Translator()
    {
        AddLanguages();
    }

    public void Reload()
    {
        _languages = new Dictionary<LanguageCode, Language.Language>();
        _localeMap = new Dictionary<string, LanguageCode>();
        AddLanguages();
    }

    private void AddLanguages()
    {
        var path = "Translation/Languages/";
        _languages.Add(LanguageCode.EN, Language.Language.FromJson(File.ReadAllText(path + "EN.json")));
        _languages.Add(LanguageCode.RU, Language.Language.FromJson(File.ReadAllText(path + "RU.json")));
        _languages.Add(LanguageCode.UA, Language.Language.FromJson(File.ReadAllText(path + "UA.json")));
        
        _localeMap.Add("en-US", LanguageCode.EN);
        _localeMap.Add("ru", LanguageCode.RU);
        _localeMap.Add("uk", LanguageCode.UA);
    }
    
    public enum LanguageCode
    {
        RU,
        EN,
        UA
    }
}