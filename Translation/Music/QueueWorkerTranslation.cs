using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class QueueWorkerTranslation
{
    [JsonProperty("empty_queue")] public string EmptyQueue { get; set; }
    
    [JsonProperty("leaving_in_n_seconds")] public string LeavingInNSeconds { get; set; }

    [JsonProperty("now_playing")] public string NowPlaying { get; set; }

    [JsonProperty("if_playback_stopped")] public string IfPlaybackStopped { get; set; }
}