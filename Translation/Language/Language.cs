using Newtonsoft.Json;
#pragma warning disable CS8618

namespace SQR.Translation.Language
{
    public partial class Language
    {
        [JsonProperty("music")]
        public Music Music { get; set; }

        [JsonProperty("dev")]
        public Dev Dev { get; set; }
    }

    public partial class Language
    {
        public static Language FromJson(string json) => JsonConvert.DeserializeObject<Language>(json, QuickType.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Language self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
    }
}