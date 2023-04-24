using System.Collections;
using System.Diagnostics;
using System.Text;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using SQR.Expections;
using SQR.Extenstions;
using SQR.Translation;
using SQR.Translation.Music;
using QueueWorker = SQR.Workers.QueueWorker;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("play", "Add track to queue")]
    public async Task PlayCommand(InteractionContext context, [Option("name", "Music to search")] string search,
        [Option("SearchSource", "Use different search engine")]
        QueueWorker.SearchSources source = QueueWorker.SearchSources.YouTube)
    {
        var language = Language.GetLanguageOrFallback(_translator, context.Locale);
        
        var music = language.Music;

        await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        await _queue.TryConnectAsync(context);

        var loadResult = await _queue.AddAsync(context, search, source);

        if (loadResult.LavalinkLoadResult.LoadResultType is LavalinkLoadResultType.LoadFailed
            or LavalinkLoadResultType.NoMatches)
        {
            throw new TrackSearchFailedException(search, true);
        }

        if (loadResult.IsPlaylist == false)
        {
            var track = loadResult.LavalinkLoadResult.Tracks.First();

            var embedSingle = new DiscordEmbedBuilder()
                .AsSQRDefault(context.Client)
                .WithTitle(music.PlayCommand.AddedToQueueSingleSuccess)
                .WithDescription(
                    string.Format(music.PlayCommand.AddedToQueueSingleDescription,
                        track.Title, track.Author, track.Length.ToString(@"hh\:mm\:ss"))
                    );

            await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedSingle));
            return;
        }

        StringBuilder stringBuilder;

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(context.Client)
            .WithTitle(string.Format(music.PlayCommand.AddedToQueuePlaylistSuccess,
            loadResult.LavalinkLoadResult.PlaylistInfo.Name));

        if (language.IsSlavicLanguage)
        {
            var parts = music.PlayCommand.SlavicParts;

            stringBuilder = new StringBuilder(
                string.Format(music.PlayCommand.AddedToQueuePlaylistDescription,
                    loadResult.LavalinkLoadResult.Tracks.Count,
                    Translator.WordForSlavicLanguage(loadResult.LavalinkLoadResult.Tracks.Count, parts.OneTrack,
                        parts.TwoTracks, parts.FiveTracks))
            );
        }
        else
        {
            stringBuilder = new StringBuilder(
                string.Format(music.PlayCommand.AddedToQueuePlaylistDescription,
                    loadResult.LavalinkLoadResult.Tracks.Count)
                );
        }

        var queueCommand = context.Client.GetApplicationCommands().GetGlobalCommand(nameof(QueueCommand))!.Mention; 
        const int displayCount = 5;
        var transformed = loadResult.LavalinkLoadResult.Tracks.Select(loadResultTrack =>
                string.Format(music.PlayCommand.AddedToQueueMessagePattern, loadResultTrack.Title,
                    loadResultTrack.Length.ToString(@"hh\:mm\:ss"), loadResultTrack.Author))
            .Take(displayCount).ToList();
        
        foreach (var content in transformed)
        {
            stringBuilder.Append(content);
        }

        if (language.IsSlavicLanguage)
        {
            var globalParts = language.Music.SlavicParts;
            stringBuilder.Append("\n" + string.Format(music.PlayCommand.MoreInQueueCommand,
                transformed.Count,
                Translator.WordForSlavicLanguage(loadResult.LavalinkLoadResult.Tracks.Count, globalParts.OneTrack,
                    globalParts.TwoTracks, globalParts.FiveTracks),
                queueCommand));
        }
        else
        {
            stringBuilder.Append("\n" + string.Format(music.PlayCommand.MoreInQueueCommand, transformed.Count, queueCommand));
        }


        embed.WithDescription(stringBuilder.ToString());

        await context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}
