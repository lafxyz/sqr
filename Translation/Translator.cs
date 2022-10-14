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
    
    //idk how to name this properly)
    /// <summary>
    /// Gets suffix for slavic language
    /// </summary>
    /// <param name="num"></param>
    /// <param name="one"></param>
    /// <param name="two"></param>
    /// <param name="five"></param>
    /// <example>SuffixForSlavicLanguage(5, "яблоко". "яблока", "яблок") => яблок</example>
    /// <example2>SuffixForSlavicLanguage(24, "яблоко". "яблока", "яблок") => яблока</example2>
    /// <returns></returns>
    public static string SuffixForSlavicLanguage(int num, string one, string two, string five)
    {
        if (num > 100) num %= 100;
        if (num is <= 20 and >= 10) return five;
        if (num > 20) num %= 10;
        return num == 1 ? one : num > 1 && num < 5 ? two : five;
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