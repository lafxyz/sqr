using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using SQR.Services;

namespace SQR.Models.Music;

public class ConnectedGuild
{
    public Guid Id = Guid.NewGuid();
    public Track? NowPlaying = null;
    public QueueService.LoopingState Looping = QueueService.LoopingState.Disabled;
    public List<Track> Queue = new();
    public InteractionContext Context = null!;
    public bool IsPaused = false;
    public Commands.Music.Music.EqPresets Preset = Commands.Music.Music.EqPresets.Default;
    public int Volume;
    public bool IsFirstTrackReceived = false;
    public bool WaitingForTracks = false;
    public DateTimeOffset WaitingForTracksSince;
    public VoteSkip VoteSkip = new();
}

public class VoteSkip
{
    public Track? RequestedTrackToSkip { get; set; }
    public List<ulong> Users { get; private set; } = new();

    public void Reset()
    {
        RequestedTrackToSkip = null;
        Users = new List<ulong>();
    }

    public bool Vote(DiscordUser user)
    {
        if (Users.Contains(user.Id))
            return false;

        Users.Add(user.Id);
        return true;
    }
}