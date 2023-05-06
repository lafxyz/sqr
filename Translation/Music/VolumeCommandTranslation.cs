using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class VolumeCommandTranslation
{
    [JsonProperty("volume_updated")]
    public string VolumeUpdated { get; set; }
}