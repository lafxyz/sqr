using System.Resources;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Expections;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("equalizer", "Band Equalizer")]
    public async Task EqualizerCommand(InteractionContext context, [Option("band", "From 0 up to 14")] int bandId, [Option("scale", "From -0,25 up to 1,0")] string scale)
    {
        var language = Translator.Languages[Translator.FallbackLanguage].Music;

        if (Translator!.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].Music;
        }

        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);

        var gain = Convert.ToSingle(scale);
        await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(bandId, gain));

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = string.Format(language.EqualizerCommand.GainUpdated, bandId, gain)
            });
    }
}