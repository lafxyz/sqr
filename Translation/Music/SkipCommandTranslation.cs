using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class SkipCommandTranslation
{
    [JsonProperty("skipped")]
    public string Skipped { get; set; }
}