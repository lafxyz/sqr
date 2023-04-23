using Newtonsoft.Json;

namespace SQR.Translation.Music.SlavicParts;

public class PlaySlavicParts
{
    [JsonProperty("one_track")]
    public string OneTrack { get; set; }
    
    [JsonProperty("two_tracks")]
    public string TwoTracks { get; set; }
    
    [JsonProperty("five_tracks")]
    public string FiveTracks { get; set; }
}