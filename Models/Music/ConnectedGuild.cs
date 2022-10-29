using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink;
using SQR.Workers;

namespace SQR.Models.Music;

public class ConnectedGuild
{
    public QueueWorker.LoopingState Looping = QueueWorker.LoopingState.NoLoop;
    public List<Track>? Queue;
    public InteractionContext? Context;
    public int Volume;
    public bool IsFirstTrackReceived = false;
    public bool WaitingForTracks = false;
    public DateTimeOffset WaitingForTracksSince;
    
    
}