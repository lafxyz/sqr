using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Language.Music;

public class PlayCommand
{
    [JsonProperty("track_search_failed")]
    public string TrackSearchFailed { get; set; }

    [JsonProperty("added_to_queue")]
    public string AddedToQueue { get; set; }

    [JsonProperty("playlist_added_to_queue")]
    public string PlaylistAddedToQueue { get; set; }

    [JsonProperty("added_to_queue_message_pattern")]
    public string AddedToQueueMessagePattern { get; set; }

    [JsonProperty("empty_queue")]
    public string EmptyQueue { get; set; }

    [JsonProperty("now_playing")]
    public string NowPlaying { get; set; }

    [JsonProperty("if_playback_stopped")]
    public string IfPlaybackStopped { get; set; }
}