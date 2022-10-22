using DisCatSharp.Entities;
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
    /// <param name="num">Input number</param>
    /// <param name="one">a word that counts as a singular thing e.g.(1) яблоко</param>
    /// <param name="two">a word that counts as a two things e.g.(2) яблока</param>
    /// <param name="five">a word that counts as a five things e.g.(5) яблок</param>
    /// <example>WordForSlavicLanguage(5, "яблоко". "яблока", "яблок") => яблок</example>
    /// <example2>WordForSlavicLanguage(24, "яблоко". "яблока", "яблок") => яблока</example2>
    public string WordForSlavicLanguage(int num, string one, string two, string five)
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
        var path = "Translation/Translations/";
        _languages.Add(LanguageCode.EN, Language.Language.FromJson(File.ReadAllText(path + "EN.json")));
        _languages.Add(LanguageCode.RU, Language.Language.FromJson(File.ReadAllText(path + "RU.json")));
        _languages.Add(LanguageCode.UA, Language.Language.FromJson(File.ReadAllText(path + "UA.json")));
        
        _localeMap.Add(DiscordLocales.AMERICAN_ENGLISH, LanguageCode.EN);
        _localeMap.Add(DiscordLocales.RUSSIAN, LanguageCode.RU);
        _localeMap.Add(DiscordLocales.UKRAINIAN, LanguageCode.UA);
    }
    
    public enum LanguageCode
    {
        RU,
        EN,
        UA
    }
}