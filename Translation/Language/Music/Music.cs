using Newtonsoft.Json;
using SQR.Translation.Language.Music.SlavicParts;

#pragma warning disable CS8618

namespace SQR.Translation.Language.Music;

public class Music
{
    [JsonProperty("slavic_parts")]
    public MusicSlavicParts SlavicParts { get; set; }
    
    [JsonProperty("general")]
    public MusicGeneral General { get; set; }

    [JsonProperty("play_command")]
    public PlayCommand PlayCommand { get; set; }

    [JsonProperty("queue_command")]
    public QueueCommand QueueCommand { get; set; }
    
    [JsonProperty("pause_command")]
    public PauseCommand PauseCommand { get; set; }
    
    [JsonProperty("loop_command")]
    public LoopCommand LoopCommand { get; set; }
    
    [JsonProperty("stop_command")]
    public StopCommand StopCommand { get; set; }
    
    [JsonProperty("equalizer_command")]
    public EqualizerCommand EqualizerCommand { get; set; }
    
    [JsonProperty("resume_command")]
    public ResumeCommand ResumeCommand { get; set; }
    
    [JsonProperty("seek_command")]
    public SeekCommand SeekCommand { get; set; }
    
    [JsonProperty("shuffle_command")]
    public ShuffleCommand ShuffleCommand { get; set; }

    [JsonProperty("skip_command")]
    public SkipCommand SkipCommand { get; set; }
    
    [JsonProperty("volume_command")]
    public VolumeCommand VolumeCommand { get; set; }
    
    [JsonProperty("equalizer_preset_command")]
    public EqualizerPresetCommand EqualizerPresetCommand { get; set; }
}