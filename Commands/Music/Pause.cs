using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("pause", "Pauses playback")]
    public async Task PauseCommand(InteractionContext context)
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
        
        await conn.PauseAsync();

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = $"Paused `{conn.CurrentState.CurrentTrack.Title}` by `{conn.CurrentState.CurrentTrack.Author}`"
            });
    }
}