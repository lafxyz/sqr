using Newtonsoft.Json;

namespace SQR.Translation.Music.SlavicParts;

public class VoteSkipSlavicParts
{
    [JsonProperty("one_vote")]
    public string OneVote { get; set; }
    
    [JsonProperty("two_votes")]
    public string TwoVotes { get; set; }
    
    [JsonProperty("five_votes")]
    public string FiveVotes { get; set; }
}