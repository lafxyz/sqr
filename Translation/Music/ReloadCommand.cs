using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class ReloadCommand
{
    [JsonProperty("reloaded")]
    public string Reloaded { get; set; }
}