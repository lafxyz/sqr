using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation;

public class DevGeneralTranslation
{
    [JsonProperty("no_access")]
    public string NoAccess { get; set; }
}