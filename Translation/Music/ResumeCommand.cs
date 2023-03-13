using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class ResumeCommand
{
    [JsonProperty("resumed")]
    public string Resumed { get; set; }
}