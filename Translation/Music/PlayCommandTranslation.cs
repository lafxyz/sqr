using Newtonsoft.Json;
using SQR.Translation.Music.SlavicParts;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class PlayCommandTranslation
{
    [JsonProperty("slavic_parts")] public PlaySlavicParts SlavicParts { get; set; }
    
    [JsonProperty("added_to_queue_single_success")] public string AddedToQueueSingleSuccess { get; set; }
    
    [JsonProperty("added_to_queue_single_description")] public string AddedToQueueSingleDescription { get; set; }

    [JsonProperty("added_to_queue_playlist_success")] public string AddedToQueuePlaylistSuccess { get; set; }
    
    [JsonProperty("added_to_queue_playlist_description")] public string AddedToQueuePlaylistDescription { get; set; }

    [JsonProperty("added_to_queue_message_pattern")] public string AddedToQueueMessagePattern { get; set; }
    
    [JsonProperty("more_in_queue_command")] public string MoreInQueueCommand { get; set; }
}