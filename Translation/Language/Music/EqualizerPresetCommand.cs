using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Language.Music;

public class EqualizerPresetCommand
{
    [JsonProperty("preset_updated")]
    public string PresetUpdated { get; set; }
}