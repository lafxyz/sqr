using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation;

public class GenericTranslation
{
    [JsonProperty("yes")] public string Yes { get; set; }
    [JsonProperty("no")] public string No { get; set; }
    
    [JsonProperty("internal_error")] public string InternalError { get; set; }
    
    [JsonProperty("external_error")] public string ExternalError { get; set; }
}