using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace SQR.Commands.Music;

[SlashCommandGroup("Music", "Music module")]
public class Music : ApplicationCommandsModule
{
    private static Queue<LavalinkTrack> _queue = new();

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

    [SlashCommand("play", "Leave from voice channel")]
    public async Task PlayCommand(InteractionContext context, [Option("name", "Music to search", false)] string search)
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
        
        var loadResult = await node.Rest.GetTracksAsync(search);
        
        if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = $"Track search failed for {search}."
                });
            return;
        }
        
        var track = loadResult.Tracks.First();

        _queue.Enqueue(track);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = $"Added to queue `{track.Title}` by `{track.Author}` ({track.Length.ToString(@"hh\:mm\:ss")})."
            });

        while (conn.IsConnected && _queue.Any())
        {
            if (conn.CurrentState.CurrentTrack == null)
            {
                var toPlay = _queue.Dequeue();

                await conn.PlayAsync(toPlay);

                await context.Channel.SendMessageAsync($"Now playing `{toPlay.Title}` by `{toPlay.Author}` ({toPlay.Length.ToString(@"hh\:mm\:ss")}).");
            }
        }
    }
}