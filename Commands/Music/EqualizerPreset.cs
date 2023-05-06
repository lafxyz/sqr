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
    [SlashCommand("equalizerpreset", "Band Equalizer presets")]
    public async Task EqualizerPresetCommand(InteractionContext context, [Option("preset", "EarRape, Bass, Pop, Default")] EqPresets preset)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).MusicTranslation;
        
        var conn = GetConnection(context);

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
            for (var i = 0; i < presetMap[preset]?.Length; i++)
            {
                await conn.AdjustEqualizerAsync(new LavalinkBandAdjustment(i, presetMap[preset]![i]));
            }
        }
        
        _queue.SetPreset(context, preset);

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(context.Client)
            .WithTitle(music.EqualizerPresetCommandTranslation.Success)
            .WithDescription(string.Format(music.EqualizerPresetCommandTranslation.PresetUpdated, preset));

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(embed));
    }
}