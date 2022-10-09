using DisCatSharp.Lavalink;
namespace SQR.Models.Music;

public class ConnectedGuild
{
    public Commands.Music.Music.LoopingState Looping = Commands.Music.Music.LoopingState.NoLoop;
    public List<Track> Queue;
    public int Volume;

}