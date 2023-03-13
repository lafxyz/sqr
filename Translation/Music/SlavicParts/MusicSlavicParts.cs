using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music.SlavicParts;

public class MusicSlavicParts
{
    [JsonProperty("one_track")]
    public string OneTrack { get; set; }
    
    [JsonProperty("two_tracks")]
    public string TwoTracks { get; set; }
    
    [JsonProperty("five_tracks")]
    public string FiveTracks { get; set; }
    
    [JsonProperty("one_second")]
    public string OneSecond { get; set; }
    
    [JsonProperty("two_seconds")]
    public string TwoSeconds { get; set; }
    
    [JsonProperty("five_seconds")]
    public string FiveSeconds { get; set; }
}