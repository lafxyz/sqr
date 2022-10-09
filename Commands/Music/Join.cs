using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace SQR.Commands.Music;

public partial class Music : ApplicationCommandsModule
{
    [SlashCommand("join", "Join a voice channel")]
    public async Task JoinVoiceCommand(InteractionContext context)
    {
        var member = context.Member;
        var voiceState = member.VoiceState;

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

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = $"Successfully connected to `{voiceState.Channel.Name}`"
            });
    }
}