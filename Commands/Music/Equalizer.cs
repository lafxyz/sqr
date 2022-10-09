using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("equalizer", "Pauses playback")]
    public async Task EqualizerCommand(InteractionContext context, [Option("band", "From 0 up to 14")] int bandId, [Option("scale", "From -0,25 up to 1,0")] string scale)
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

        var gain = Convert.ToSingle(scale);
        await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(bandId, gain));

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = $"Band {bandId} gain set to {gain}"
            });
    }
}