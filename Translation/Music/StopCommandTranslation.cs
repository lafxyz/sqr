using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class StopCommandTranslation
{
    [JsonProperty("disconnected")]
    public string Disconnected { get; set; }
}