using Newtonsoft.Json;
#pragma warning disable CS8618

namespace SQR.Translation.Language.Music.SlavicParts;

public class MusicSlavicParts
{
    [JsonProperty("one_track")]
    public string OneTrack { get; set; }
    
    [JsonProperty("two_tracks")]
    public string TwoTracks { get; set; }
    
    [JsonProperty("five_tracks")]
    public string FiveTracks { get; set; }
}