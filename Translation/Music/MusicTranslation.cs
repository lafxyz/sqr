using Newtonsoft.Json;
using SQR.Translation.Music.SlavicParts;

#pragma warning disable CS8618

namespace SQR.Translation.Music;

public class MusicTranslation
{
    [JsonProperty("slavic_parts")]
    public MusicSlavicParts SlavicParts { get; set; }
    
    [JsonProperty("play_command")]
    public PlayCommandTranslation PlayCommandTranslation { get; set; }

    [JsonProperty("queue_command")]
    public QueueCommandTranslation QueueCommandTranslation { get; set; }
    
    [JsonProperty("queue_worker")]
    public QueueWorkerTranslation QueueWorkerTranslation { get; set; }
    
    [JsonProperty("pause_command")]
    public PauseCommandTranslation PauseCommandTranslation { get; set; }
    
    [JsonProperty("loop_command")]
    public LoopCommandTranslation LoopCommandTranslation { get; set; }
    
    [JsonProperty("stop_command")]
    public StopCommandTranslation StopCommandTranslation { get; set; }
    
    [JsonProperty("equalizer_command")]
    public EqualizerCommandTranslation EqualizerCommandTranslation { get; set; }
    
    [JsonProperty("resume_command")]
    public ResumeCommandTranslation ResumeCommandTranslation { get; set; }
    
    [JsonProperty("seek_command")]
    public SeekCommandTranslation SeekCommandTranslation { get; set; }
    
    [JsonProperty("shuffle_command")]
    public ShuffleCommandTranslation ShuffleCommandTranslation { get; set; }

    [JsonProperty("skip_command")]
    public SkipCommandTranslation SkipCommandTranslation { get; set; }
    
    [JsonProperty("volume_command")]
    public VolumeCommandTranslation VolumeCommandTranslation { get; set; }
    
    [JsonProperty("equalizer_preset_command")]
    public EqualizerPresetCommandTranslation EqualizerPresetCommandTranslation { get; set; }

    [JsonProperty("status_command")]
    public StatusCommandTranslation StatusCommandTranslation { get; set; }
    
    [JsonProperty("vote_skip_command")]
    public VoteSkipCommandTranslation VoteSkipCommandTranslation { get; set; }
}