using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Language.Music;

public class VolumeCommand
{
    [JsonProperty("volume_updated")]
    public string VolumeUpdated { get; set; }
}