using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class EqualizerPresetCommandTranslation
{
    [JsonProperty("success")] public string Success { get; set; }
    [JsonProperty("preset_updated")] public string PresetUpdated { get; set; }
}