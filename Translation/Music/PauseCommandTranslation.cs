using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class PauseCommandTranslation
{
    [JsonProperty("success")] public string Success { get; set; }
    [JsonProperty("paused")] public string Paused { get; set; }
}