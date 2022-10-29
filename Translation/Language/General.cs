using Newtonsoft.Json;
#pragma warning disable CS8618

namespace SQR.Translation.Language;

public class General
{
    [JsonProperty("internal_error")] 
    public string InternalError { get; set; }
    
    [JsonProperty("external_error")] 
    public string ExternalError { get; set; }
}