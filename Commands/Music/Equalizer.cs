using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using SQR.Extenstions;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("equalizer", "Band Equalizer")]
    public async Task EqualizerCommand(InteractionContext context, [Option("band", "From 0 up to 14")] int bandId, [Option("scale", "From -0,25 up to 1,0")] string scale)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).Music;

        var conn = GetConnection(context);

        var gain = Convert.ToSingle(scale);
        await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(bandId, gain));

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault()
            .WithTitle(music.EqualizerCommand.Success)
            .WithDescription(string.Format(music.EqualizerCommand.GainUpdated, bandId, gain));

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(embed));
    }
}