using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class QueueCommandTranslation
{
    [JsonProperty("queue_message_pattern")] public string QueueMessagePattern { get; set; }

    [JsonProperty("now_playing")] public string NowPlaying { get; set; }
    
    [JsonProperty("current_page")] public string CurrentPage { get; set; }
}