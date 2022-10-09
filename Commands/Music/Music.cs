using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.VoiceNext;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace SQR.Commands.Music;

[SlashCommandGroup("Music", "Music module")]
public class Music : ApplicationCommandsModule
{
    [SlashCommand("join", "Join a voice channel")]
    public async Task JoinVoiceCommand(InteractionContext context, [Option("eh", "ehh", false)] DiscordUser eh)
    {
        var member = await eh.ConvertToMember(context.Guild);
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

        await voiceState.Channel.ConnectAsync();

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = $"Successfully connected to `{voiceState.Channel.Name}`"
            });
    }
    
    [SlashCommand("leave", "Join a voice channel")]
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

        context.Client.GetVoiceNext().GetConnection(context.Guild).Disconnect();

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
        new DiscordInteractionResponseBuilder
        {
            IsEphemeral = true,
            Content = $"Successfully disconnected from `{voiceState.Channel.Name}`"
        });
    }
}