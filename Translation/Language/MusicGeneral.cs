using Newtonsoft.Json;
#pragma warning disable CS8618

namespace SQR.Translation.Language;

public class MusicGeneral
{
    [JsonProperty("lavalink_is_not_connected")]
    public string LavalinkIsNotConnected { get; set; }

    [JsonProperty("not_in_voice")]
    public string NotInVoice { get; set; }
    
    [JsonProperty("different_voice")]
    public string DifferentVoice { get; set; }
}