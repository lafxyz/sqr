using Newtonsoft.Json;
#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class StatusCommand
{
    [JsonProperty("title_format")] public string TitleFormat { get; set; }
    [JsonProperty("is_paused")] public string IsPaused { get; set; }
    [JsonProperty("volume")] public string Volume { get; set; }
    [JsonProperty("preset")] public string Preset { get; set; }
    [JsonProperty("playing")] public string Playing { get; set; }
    [JsonProperty("paused")] public string Paused { get; set; }
    [JsonProperty("loop")] public string Loop { get; set; }
    [JsonProperty("no_loop")] public string NoLoop { get; set; }
    [JsonProperty("loop_queue")] public string LoopQueue { get; set; }
    [JsonProperty("loop_track")] public string LoopTrack { get; set; }
    [JsonProperty("fill_symbol")] public string FillSymbol { get; set; }
    [JsonProperty("rest_symbol")] public string RestSymbol { get; set; }
    
    [JsonProperty("presets")] public Presets Presets { get; set; }
}

public class Presets
{
    [JsonProperty("ear_rape")] public string EarRape { get; set; }
    [JsonProperty("bass")] public string Bass { get; set; }
    [JsonProperty("pop")] public string Pop { get; set; }
    [JsonProperty("default")] public string Default { get; set; }
}

