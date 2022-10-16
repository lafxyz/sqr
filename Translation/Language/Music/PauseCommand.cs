using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Language.Music;

public class PauseCommand
{
    [JsonProperty("paused")]
    public string Paused { get; set; }
}