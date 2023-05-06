using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class ReloadCommandTranslation
{
    [JsonProperty("reloaded")]
    public string Reloaded { get; set; }
}