using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation;

public class Exceptions
{
    [JsonProperty("client_is_not_connected")] public string ClientIsNotConnected { get; set; }
    [JsonProperty("different_voice")] public string DifferentVoice { get; set; }
    [JsonProperty("lavalink_is_not_connected")] public string LavalinkIsNotConnected { get; set; }
    [JsonProperty("not_in_voice")] public string NotInVoice { get; set; }
    [JsonProperty("track_search_failed")] public string TrackSearchFailed { get; set; }
    [JsonProperty("nothing_is_playing")] public string NothingIsPlaying { get; set; }
    [JsonProperty("already_voted")] public string AlreadyVoted { get; set; }
    
}