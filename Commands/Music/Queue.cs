using System.Text;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace SQR.Commands.Music;

public partial class Music : ApplicationCommandsModule
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
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
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

        var stringBuilder = new StringBuilder($"In queue: ({_connectionsWithQueue[conn].Count})");
        foreach (var track in _connectionsWithQueue[conn])
        {
            stringBuilder.Append($"\n - `{track.Title}` by `{track.Author}` ({track.Length.ToString(@"hh\:mm\:ss")})");
        }
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = stringBuilder.ToString()
            });
    }
}