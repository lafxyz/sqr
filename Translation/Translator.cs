using DisCatSharp.Entities;

namespace SQR.Translation;

public class Translator
{
    public static LanguageCode FallbackLanguage;
    
    public Dictionary<LanguageCode, Language> Languages => _languages;
    public Dictionary<string, LanguageCode> LocaleMap => _localeMap;
    
    private Dictionary<string, LanguageCode> _localeMap = new();
    private Dictionary<LanguageCode, Language> _languages = new();
    private readonly string LanguagesPath = "Translation/Languages";
    
    public enum LanguageCode
    {
        RU,
        EN,
        UA
    }

    public Translator()
    {
        AddLanguages();
    }
    
    /// <param name="x">Input number</param>
    /// <param name="a">a word that counts as a singular thing e.g. 1 яблоко</param>
    /// <param name="b">a word that counts as a b things e.g. 2 яблока</param>
    /// <param name="c">a word that counts as a c things e.g. 5 яблок</param>
    /// <example>WordForSlavicLanguage(5, "яблоко". "яблока", "яблок") => яблок</example>
    /// <example2>WordForSlavicLanguage(24, "яблоко". "яблока", "яблок") => яблока</example2>
    public static string WordForSlavicLanguage(int x, string a, string b, string c)
    {
        if (x > 100) x %= 100;
        if (x is <= 20 and >= 10) return c;
        if (x > 20) x %= 10;
        return x == 1 ? a : x is > 1 and < 5 ? b : c;
    }

    public void Reload()
    {
        _languages = new Dictionary<LanguageCode, Language>();
        _localeMap = new Dictionary<string, LanguageCode>();
        AddLanguages();
    }

    private void AddLanguages()
    {
        _languages.Add(LanguageCode.EN, Language.FromJson(File.ReadAllText(LanguagesPath + "/EN.json")));
        _languages.Add(LanguageCode.RU, Language.FromJson(File.ReadAllText(LanguagesPath + "/RU.json")));
        _languages.Add(LanguageCode.UA, Language.FromJson(File.ReadAllText(LanguagesPath + "/UA.json")));

        _localeMap.Add(DiscordLocales.AMERICAN_ENGLISH, LanguageCode.EN);
        _localeMap.Add(DiscordLocales.RUSSIAN, LanguageCode.RU);
        _localeMap.Add(DiscordLocales.UKRAINIAN, LanguageCode.UA);
    }
    
}