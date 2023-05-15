using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation;

public class TemporaryTranslation
{
    [JsonProperty("footer")] public string Footer { get; set; }
    [JsonProperty("army_lines")] public string[] ArmyLines { get; set; }
}