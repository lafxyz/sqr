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
    [SlashCommand("equalizerpreset", "Band Equalizer presets")]
    public async Task EqualizerPresetCommand(InteractionContext context, [Option("preset", "EarRape, Bass, Pop, Default")] EqPresets preset)
    {
        var language = Translator.Languages[Translator.FallbackLanguage].Music;

        if (Translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].Music;
        }
        
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        
    
        var presetMap = new Dictionary<EqPresets, float[]?>
        {
            { EqPresets.EarRape, new[] { 1f, 1f, 1f, 1f, -0.25f, -0.25f, -0.25f, -0.25f, -0.25f, -0.25f, -0.25f, 1f, 1f, 1f, 1f } },
            { EqPresets.Bass, new[] { 0.10f, 0.10f, 0.05f, 0.05f, 0.05f, -0.05f, -0.05f, 0f, -0.05f, -0.05f, 0f, 0.05f, 0.05f, 0.10f, 0.10f } },
            { EqPresets.Pop, new[] { -0.01f, -0.01f, 0f, 0.01f, 0.02f, 0.05f, 0.07f, 0.10f, 0.07f, 0.05f, 0.02f, 0.01f, 0f, -0.01f, -0.01f } },
            { EqPresets.Default, null }
        };

        if (presetMap[preset] == null)
        {
            await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment());
        }
        else
        {
            for (int i = 0; i < presetMap[preset]?.Length; i++)
            {
                await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(i, presetMap[preset]![i]));
            }
        }
        
        Queue.SetPreset(context, preset);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = string.Format(language.EqualizerPresetCommand.PresetUpdated, preset)
            });
    }
}