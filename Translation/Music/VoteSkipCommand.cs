using Newtonsoft.Json;
using SQR.Translation.Music.SlavicParts;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class VoteSkipCommand
{
    [JsonProperty("slavic_parts")] public VoteSkipSlavicParts SlavicParts { get; set; }
    
    [JsonProperty("threshold_passed")] public string ThresholdPassed { get; set; }
    
    [JsonProperty("voted")] public string Voted { get; set; }
}