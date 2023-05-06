using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class SeekCommandTranslation
{
    [JsonProperty("parse_failed_comment")]
    public string ParseFailedComment { get; set; }
    
    [JsonProperty("seeked")]
    public string Seeked { get; set; }
}