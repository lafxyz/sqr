using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class ResumeCommandTranslation
{
    [JsonProperty("resumed")]
    public string Resumed { get; set; }
}