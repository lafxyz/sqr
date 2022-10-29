using Newtonsoft.Json;
#pragma warning disable CS8618

namespace SQR.Translation.Language
{
    public partial class Language
    {
        [JsonProperty("is_slavic_language")]
        public bool IsSlavicLanguage { get; set; }

        [JsonProperty("general")] 
        public General General { get; set; }

        [JsonProperty("music")]
        public Music.Music Music { get; set; }

        [JsonProperty("dev")]
        public Dev Dev { get; set; }
    }

    public partial class Language
    {
        public static Language FromJson(string json) => JsonConvert.DeserializeObject<Language>(json, QuickType.Converter.Settings) ?? throw new ArgumentException(null, nameof(json));
    }

    public static class Serialize
    {
        public static string ToJson(this Language self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
    }
}