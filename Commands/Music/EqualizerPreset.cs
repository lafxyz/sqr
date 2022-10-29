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
    [SlashCommand("equalizerpreset", "Band Equalizer presets")]
    public async Task EqualizerPresetCommand(InteractionContext context, [Option("preset", "EarRape, Bass, Pop, Default")] EqPresets preset)
    {
        var language = Translator.Languages[Translator.LanguageCode.EN].Music;

        if (Translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].Music;
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
    
        var aaa = new Dictionary<EqPresets, float[]?>
        {
            { EqPresets.EarRape, new[] { 1f, 1f, 1f, 1f, -0.25f, -0.25f, -0.25f, -0.25f, -0.25f, -0.25f, -0.25f, 1f, 1f, 1f, 1f } },
            { EqPresets.Bass, new[] { 0.10f, 0.10f, 0.05f, 0.05f, 0.05f, -0.05f, -0.05f, 0f, -0.05f, -0.05f, 0f, 0.05f, 0.05f, 0.10f, 0.10f } },
            { EqPresets.Pop, new[] { -0.01f, -0.01f, 0f, 0.01f, 0.02f, 0.05f, 0.07f, 0.10f, 0.07f, 0.05f, 0.02f, 0.01f, 0f, -0.01f, -0.01f } },
            { EqPresets.Default, null }
        };

        if (aaa[preset] == null)
        {
            await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment());
        }
        else
        {
            for (int i = 0; i < aaa[preset]?.Length; i++)
            {
                await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(i, aaa[preset]![i]));
            }
        }

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = string.Format(language.EqualizerPresetCommand.PresetUpdated, preset)
            });
    }
}