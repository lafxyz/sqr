using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class StopCommand
{
    [JsonProperty("disconnected")]
    public string Disconnected { get; set; }
}