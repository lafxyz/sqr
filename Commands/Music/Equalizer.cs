using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using SQR.Attributes;
using SQR.Extenstions;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("equalizer", "Band Equalizer")]
    [RequireLavalinkConnection]
    [BotAndUserMustBeInTheSameVoiceChannel]
    [UserMustBeInVoiceChannel]
    public async Task EqualizerCommand(InteractionContext context, [Option("band", "From 0 up to 14")] int bandId, [Option("scale", "From -0,25 up to 1,0")] string scale)
    {
        var language = Language.GetLanguageOrFallback(_translator, context.Locale);
        var music = language.MusicTranslation;
        
        var conn = GetConnection(context);

        var gain = Convert.ToSingle(scale);
        await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(bandId, gain));

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(context.Client, language)
            .WithTitle(music.EqualizerCommandTranslation.Success)
            .WithDescription(string.Format(music.EqualizerCommandTranslation.GainUpdated, bandId, gain));

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(embed));
    }
}