using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Language.Music;

public class QueueCommand
{
    [JsonProperty("queue_message_pattern")]
    public string QueueMessagePattern { get; set; }

    [JsonProperty("now_playing")]
    public string NowPlaying { get; set; }
}