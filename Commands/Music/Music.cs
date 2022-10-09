using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Lavalink;
using SQR.Models.Music;

namespace SQR.Commands.Music;

[SlashCommandGroup("Music", "Music module")]
public partial class Music : ApplicationCommandsModule
{
    private static Dictionary<LavalinkGuildConnection, ConnectedGuild> _servers = new();

    private async Task DisconnectAsync(LavalinkGuildConnection connection)
    {
        await connection.DisconnectAsync();
        _servers.Remove(connection);
    }
    
    public enum SearchSources
    {
        [ChoiceName("YouTube")]
        YouTube,
        [ChoiceName("Spotify")]
        Spotify,
        [ChoiceName("AppleMusic")]
        AppleMusic,
        [ChoiceName("SoundCloud")]
        SoundCloud
    }
    
    public enum LoopingState
    {
        [ChoiceName("Without looping")]
        NoLoop,
        [ChoiceName("Loop current track")]
        LoopTrack,
        [ChoiceName("Loop current queue")]
        LoopQueue
    }
}