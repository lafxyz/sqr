using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation
{
    public partial class Language
    {
        [JsonProperty("is_slavic_language")] public bool IsSlavicLanguage { get; set; }
        
        [JsonProperty("temporary")] public TemporaryTranslation Temporary { get; set; }
        
        [JsonProperty("exceptions")] public Exceptions Exceptions { get; set; }

        [JsonProperty("generic")] public GenericTranslation GenericTranslation { get; set; }

        [JsonProperty("music")] public Music.MusicTranslation MusicTranslation { get; set; }

        [JsonProperty("dev")] public DevTranslation DevTranslation { get; set; }
    }

    public partial class Language
    {
        public static Language GetLanguageOrFallback(Translator translator, string locale)
        {
            var language = translator.Languages[Translator.FallbackLanguage];

            if (translator.LocaleMap.TryGetValue(locale, out var languageCode))
            {
                language = translator.Languages[languageCode];
            }

            return language;
        }
        public static Language FromJson(string json) => JsonConvert.DeserializeObject<Language>(json, Converter.Settings) ?? throw new ArgumentException(null, nameof(json));
    }

    public static class Serialize
    {
        public static string ToJson(this Language self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}