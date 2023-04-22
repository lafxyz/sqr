using DisCatSharp.ApplicationCommands.Context;
using SQR.Workers;

namespace SQR.Models.Music;

public class ConnectedGuild
{
    public Guid Id = Guid.NewGuid();
    public Track? NowPlaying = null;
    public QueueWorker.LoopingState Looping = QueueWorker.LoopingState.Disabled;
    public List<Track> Queue = new();
    public InteractionContext Context = null!;
    public bool IsPaused = false;
    public Commands.Music.Music.EqPresets Preset = Commands.Music.Music.EqPresets.Default;
    public int Volume;
    public bool IsFirstTrackReceived = false;
    public bool WaitingForTracks = false;
    public DateTimeOffset WaitingForTracksSince;

}