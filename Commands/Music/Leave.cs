using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace SQR.Commands.Music;

public partial class Music : ApplicationCommandsModule
{
    [SlashCommand("leave", "Leave from voice channel")]
    public async Task LeaveVoiceCommand(InteractionContext context)
    {
        var voiceState = context.Member.VoiceState;

        if (voiceState is null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = "You're not in voice channel"
                });
            return;
        }

        if (voiceState.Channel != context.Guild.CurrentMember.VoiceState.Channel)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = "You're in different voice channel"
                });
            return;
        }
        
        var lava = context.Client.GetLavalink();
        if (!lava.ConnectedNodes.Any())
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = "The Lavalink connection is not established"
                });
            return;
        }

        var node = lava.ConnectedNodes.Values.First();
        await node.ConnectAsync(voiceState.Channel);
        
        var conn = node.GetGuildConnection(voiceState.Guild);

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
        
        await conn.DisconnectAsync();

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = $"Successfully disconnected from `{voiceState.Channel.Name}`"
            });
    }
}