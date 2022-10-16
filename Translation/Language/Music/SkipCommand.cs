using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Language.Music;

public class SkipCommand
{
    [JsonProperty("skipped")]
    public string Skipped { get; set; }
}