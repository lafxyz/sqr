using Newtonsoft.Json;

namespace SQR.Translation.Language;

public partial class General
{
    [JsonProperty("internal_error")] 
    public string InternalError { get; set; }
    
    [JsonProperty("external_error")] 
    public string ExternalError { get; set; }
}