using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class LoopCommand
{
    [JsonProperty("success")] public string Success { get; set; }
    [JsonProperty("no_loop")] public string NoLoop { get; set; }
    
    [JsonProperty("loop_track")] public string LoopTrack { get; set; }
    
    [JsonProperty("loop_queue")] public string LoopQueue { get; set; }

    
}