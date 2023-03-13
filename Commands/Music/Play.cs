using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Exceptions;
using DisCatSharp.Lavalink;
using SQR.Expections;
using SQR.Translation;
using SQR.Workers;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("play", "Add track to queue")]
    public async Task PlayCommand(InteractionContext context, [Option("name", "Music to search")] string search,
        [Option("SearchSource", "Use different search engine")]
        QueueWorker.SearchSources source = QueueWorker.SearchSources.YouTube)
    {
        var isSlavic = Translator.Languages[Translator.FallbackLanguage].IsSlavicLanguage;
        
        var language = Translator.Languages[Translator.LanguageCode.EN];

        if (Translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]];
            isSlavic = Translator.Languages[Translator.LocaleMap[context.Locale]].IsSlavicLanguage;
        }

        var music = language.Music;

        await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        await Queue.TryConnectAsync(context);

        var loadResult = await Queue.AddAsync(context, search, source);

        if (loadResult.LavalinkLoadResult.LoadResultType is LavalinkLoadResultType.LoadFailed
            or LavalinkLoadResultType.NoMatches)
        {
            throw new TrackSearchFailedException(search, true);
        }

        if (loadResult.IsPlaylist == false)
        {
            var track = loadResult.LavalinkLoadResult.Tracks.First();
            await context.EditResponseAsync(new DiscordWebhookBuilder
            {
                Content = string.Format(music.PlayCommand.AddedToQueue,
                    track.Title, track.Author, track.Length.ToString(@"hh\:mm\:ss"))
            });
            return;
        }

        StringBuilder stringBuilder;

        if (isSlavic)
        {
            var parts = music.SlavicParts;
            stringBuilder = new StringBuilder(string.Format(music.PlayCommand.PlaylistAddedToQueue,
                loadResult.LavalinkLoadResult.Tracks.Count,
                Translator.WordForSlavicLanguage(loadResult.LavalinkLoadResult.Tracks.Count, parts.OneTrack,
                    parts.TwoTracks, parts.FiveTracks),
                loadResult.LavalinkLoadResult.PlaylistInfo.Name));
        }
        else
        {
            stringBuilder = new StringBuilder(string.Format(music.PlayCommand.PlaylistAddedToQueue,
                loadResult.LavalinkLoadResult.Tracks.Count, loadResult.LavalinkLoadResult.PlaylistInfo.Name));
        }

        foreach (var content in loadResult.LavalinkLoadResult.Tracks.Select(loadResultTrack =>
                         string.Format(music.PlayCommand.AddedToQueueMessagePattern, loadResultTrack.Title,
                             loadResultTrack.Length.ToString(@"hh\:mm\:ss"), loadResultTrack.Author))
                     .Where(content => stringBuilder.Length + content.Length <= 2000))
        {
            stringBuilder.Append(content);
        }

        await context.EditResponseAsync(new DiscordWebhookBuilder
        {
            Content = stringBuilder.ToString()
        });
    }
}
