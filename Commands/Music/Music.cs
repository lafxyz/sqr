using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace SQR.Commands.Music;

[SlashCommandGroup("Music", "Music module")]
public partial class Music : ApplicationCommandsModule
{
    private static Dictionary<LavalinkGuildConnection, List<LavalinkTrack>> _connectionsWithQueue = new();
}