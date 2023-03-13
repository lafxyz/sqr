using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using SQR.Expections;
using SQR.Extenstions;
using SQR.Translation;
using SQR.Utilities;
using SQR.Workers;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("status", "Displays current playback and settings")]
    public async Task StatusCommand(InteractionContext context)
    {
        var language = Translator.Languages[Translator.FallbackLanguage];

        if (Translator.LocaleMap.TryGetValue(context.Locale, out var value))
        {
            language = Translator.Languages[value];
        }

        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);

        if (conn.CurrentState.CurrentTrack is null)
            throw new CurrentTrackIsNullException();

        var connectedGuild = await Queue.GetConnectedGuild(conn);
        
        var loopMap = new Dictionary<QueueWorker.LoopingState, string>
        {
            { QueueWorker.LoopingState.NoLoop, language.Music.StatusCommand.NoLoop },
            { QueueWorker.LoopingState.LoopTrack, language.Music.StatusCommand.LoopTrack },
            { QueueWorker.LoopingState.LoopQueue, language.Music.StatusCommand.LoopQueue }
        };
        
        var presetsLang = language.Music.StatusCommand.Presets;
        var presetMap = new Dictionary<EqPresets, string>()
        {
            { EqPresets.EarRape, presetsLang.EarRape },
            { EqPresets.Bass, presetsLang.Bass },
            { EqPresets.Pop, presetsLang.Pop },
            { EqPresets.Default, presetsLang.Default }
        };

        var statusCommandLang = language.Music.StatusCommand;
        var embed = new DiscordEmbedBuilder
        {
            Title = string.Format(statusCommandLang.TitleFormat, 
                conn.CurrentState.CurrentTrack.Title, conn.CurrentState.CurrentTrack.Author),
            Description = await BuildDescription(conn, language)
        };
        embed.AsSQRDefault();
        embed.AddField(new DiscordEmbedField(statusCommandLang.IsPaused,
            connectedGuild.IsPaused ? language.Generic.Yes : language.Generic.No, true));
        embed.AddField(new DiscordEmbedField(statusCommandLang.Volume,
            $"{connectedGuild.Volume}%", true));
        embed.AddField(EmbedUtilites.EmptyEmbedFiledInline);
        embed.AddField(new DiscordEmbedField(statusCommandLang.Loop,
            $"{loopMap[connectedGuild.Looping]}", true));
        embed.AddField(new DiscordEmbedField(statusCommandLang.Preset,
            $"{presetMap[connectedGuild.Preset]}", true));
        embed.AddField(EmbedUtilites.EmptyEmbedFiledInline);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(embed.Build()));
    }
    
    private async Task<string> BuildDescription(LavalinkGuildConnection conn, Language lang)
    {
        var connectedGuild = await Queue.GetConnectedGuild(conn);

        var statusCommandLang = lang.Music.StatusCommand;
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