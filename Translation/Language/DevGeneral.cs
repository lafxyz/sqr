using Newtonsoft.Json;
#pragma warning disable CS8618

namespace SQR.Translation.Language;

public class DevGeneral
{
    [JsonProperty("no_access")]
    public string NoAccess { get; set; }
}