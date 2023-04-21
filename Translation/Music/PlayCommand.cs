using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class PlayCommand
{
    [JsonProperty("added_to_queue_single_success")] public string AddedToQueueSingleSuccess { get; set; }
    [JsonProperty("added_to_queue_single_description")] public string AddedToQueueSingleDescription { get; set; }

    [JsonProperty("added_to_queue_playlist_success")] public string AddedToQueuePlaylistSuccess { get; set; }
    
    [JsonProperty("added_to_queue_playlist_description")] public string AddedToQueuePlaylistDescription { get; set; }

    [JsonProperty("added_to_queue_message_pattern")] public string AddedToQueueMessagePattern { get; set; }
}