using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class EqualizerCommand
{
    [JsonProperty("success")] public string Success { get; set; }
    [JsonProperty("gain_updated")] public string GainUpdated { get; set; }
}