using Newtonsoft.Json;
#pragma warning disable CS8618

namespace SQR.Translation.Language;

public class LoopCommand
{
    [JsonProperty("no_loop")]
    public string NoLoop { get; set; }
    
    [JsonProperty("loop_track")]
    public string LoopTrack { get; set; }
    
    [JsonProperty("loop_queue")]
    public string LoopQueue { get; set; }

    
}