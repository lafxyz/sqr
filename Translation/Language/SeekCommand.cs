using Newtonsoft.Json;
#pragma warning disable CS8618

namespace SQR.Translation.Language;

public class SeekCommand
{
    [JsonProperty("parse_failed")]
    public string ParseFailed { get; set; }
    
    [JsonProperty("seeked")]
    public string Seeked { get; set; }
}