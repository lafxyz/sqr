using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("queue", "Display queue")]
    public async Task QueueCommand(InteractionContext context)
    {
        if (context.Member.VoiceState?.Channel is null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = "You're not in voice channel"
                });
        }
        
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState?.Guild);
        
        if (conn == null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = "Lavalink is not connected."
                });
            return;
        }

        var currentTrack = conn.CurrentState.CurrentTrack;
        var stringBuilder = new StringBuilder($"Now playing: `{currentTrack.Title}` by `{currentTrack.Author}` " +
                                              $"({conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss")} - {currentTrack.Length.ToString(@"hh\:mm\:ss")})" +
                                              $"\n\nIn queue: ({_servers[conn].Queue.Count})");
        
        for (var index = 0; index < _servers[conn].Queue.Count; index++)
        {
            var track = _servers[conn].Queue[index];
            var lavalinkTrack = track.LavalinkTrack;
            stringBuilder.Append($"\n**{lavalinkTrack.Author}** - **{lavalinkTrack.Title}**\n> `{lavalinkTrack.Length.ToString(@"hh\:mm\:ss")}` | Added by {lavalinkTrack.DiscordUser.Mention}");
        }

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = stringBuilder.ToString()
            });
    }
}
