using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using SQR.Exceptions;
using SQR.Extenstions;
using SQR.Services;
using SQR.Translation;
using SQR.Utilities;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("status", "Displays current playback and settings")]
    public async Task StatusCommand(InteractionContext context)
    {
        var language = Language.GetLanguageOrFallback(_translator, context.Locale);

        var conn = GetConnection(context);

        if (conn.CurrentState.CurrentTrack is null)
            throw new CurrentTrackIsNullException();

        var connectedGuild = await _queue.GetConnectedGuild(conn);
        
        var loopMap = new Dictionary<QueueService.LoopingState, string>
        {
            { QueueService.LoopingState.Disabled, language.MusicTranslation.StatusCommandTranslation.NoLoop },
            { QueueService.LoopingState.Single, language.MusicTranslation.StatusCommandTranslation.LoopTrack },
            { QueueService.LoopingState.Queue, language.MusicTranslation.StatusCommandTranslation.LoopQueue }
        };
        
        var presetsLang = language.MusicTranslation.StatusCommandTranslation.Presets;
        var presetMap = new Dictionary<EqPresets, string>()
        {
            { EqPresets.EarRape, presetsLang.EarRape },
            { EqPresets.Bass, presetsLang.Bass },
            { EqPresets.Pop, presetsLang.Pop },
            { EqPresets.Default, presetsLang.Default }
        };

        var statusCommandLang = language.MusicTranslation.StatusCommandTranslation;
        var embed = new DiscordEmbedBuilder()
            .WithTitle(string.Format(statusCommandLang.TitleFormat,
            conn.CurrentState.CurrentTrack.Title, conn.CurrentState.CurrentTrack.Author))
            .WithDescription(await BuildDescription(conn, language))
            .AsSQRDefault(context.Client)
            .AddField(new DiscordEmbedField(statusCommandLang.IsPaused, 
                connectedGuild.IsPaused ? language.GenericTranslation.Yes : language.GenericTranslation.No, true))
            .AddField(new DiscordEmbedField(statusCommandLang.Volume, 
                $"{connectedGuild.Volume}%", true))
            .AddEmptyField(true)
            .AddField(new DiscordEmbedField(statusCommandLang.Loop,
                $"{loopMap[connectedGuild.Looping]}", true))
            .AddField(new DiscordEmbedField(statusCommandLang.Preset,
                $"{presetMap[connectedGuild.Preset]}", true))
            .AddEmptyField(true);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(embed.Build()));
    }
    
    private async Task<string> BuildDescription(LavalinkGuildConnection conn, Language lang)
    {
        var connectedGuild = await _queue.GetConnectedGuild(conn);

        var statusCommandLang = lang.MusicTranslation.StatusCommandTranslation;
        var fillPercentage = conn.CurrentState.PlaybackPosition / conn.CurrentState.CurrentTrack.Length;
        return string.Format("{0} {1:d02}:{2:d02} {3} {4:d02}:{5:d02}",
            (connectedGuild.IsPaused ? statusCommandLang.Paused : statusCommandLang.Playing) + " ",
            (int)conn.CurrentState.PlaybackPosition.TotalMinutes,
            conn.CurrentState.PlaybackPosition.Seconds,
            StringUtilities.ProgressBar(statusCommandLang.FillSymbol[0], statusCommandLang.RestSymbol[0],
                                            fillPercentage, 16),
            (int)conn.CurrentState.CurrentTrack.Length.TotalMinutes,
            conn.CurrentState.CurrentTrack.Length.Seconds
        );
    }
}