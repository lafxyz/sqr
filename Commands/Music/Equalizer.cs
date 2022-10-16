using System.Resources;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("equalizer", "Band Equalizer")]
    public async Task EqualizerCommand(InteractionContext context, [Option("band", "From 0 up to 14")] int bandId, [Option("scale", "From -0,25 up to 1,0")] string scale)
    {
        var scope = context.Services.CreateScope();
        var translator = scope.ServiceProvider.GetService<Translator>();

        var isSlavic = translator?.Languages[Translator.LanguageCode.EN].IsSlavicLanguage;

        var language = translator?.Languages[Translator.LanguageCode.EN].Music;

        if (translator!.LocaleMap.ContainsKey(context.Locale))
        {
            language = translator.Languages[translator.LocaleMap[context.Locale]].Music;
        }

        var voiceState = context.Member.VoiceState;
        if (voiceState is null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.General.NotInVoice
                });
        }

        if (context.Guild.CurrentMember.VoiceState != null && voiceState.Channel != context.Guild.CurrentMember.VoiceState.Channel)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.General.DifferentVoice
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
                    Content = language.General.LavalinkIsNotConnected
                });
            return;
        }

        var gain = Convert.ToSingle(scale);
        await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(bandId, gain));

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = string.Format(language.EqualizerCommand.GainUpdated, bandId, gain)
            });
    }
}