using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Models.Music;
using SQR.Translation;
using SQR.Workers;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("play", "Add track to queue")]
    public async Task PlayCommand(InteractionContext context, [Option("name", "Music to search", false)] string search,
        [Option("SearchSource", "Use different search engine")]
        QueueWorker.SearchSources source = QueueWorker.SearchSources.YouTube)
    {
        var scope = context.Services.CreateScope();
        var translator = scope.ServiceProvider.GetService<Translator>();
        var queue = scope.ServiceProvider.GetService<QueueWorker>();
        
        var isSlavic = translator!.Languages[Translator.LanguageCode.EN].IsSlavicLanguage;

        var language = translator.Languages[Translator.LanguageCode.EN].Music;

        if (translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = translator.Languages[translator.LocaleMap[context.Locale]].Music;
            isSlavic = translator.Languages[translator.LocaleMap[context.Locale]].IsSlavicLanguage;
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

        if (context.Guild.CurrentMember.VoiceState != null && voiceState?.Channel != context.Guild.CurrentMember.VoiceState.Channel)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.General.DifferentVoice
                });
            return;
        }

        var isConnected = await queue?.TryConnectAsync(context)!;

        var loadResult = await queue.AddAsync(context, search, source);
        
        if (loadResult.LavalinkLoadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = string.Format(language.PlayCommand.TrackSearchFailed, search)
                });
            return;
        }
        if (loadResult.IsPlaylist == false)
        {
            var track = loadResult.LavalinkLoadResult.Tracks.First();

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = string.Format(language.PlayCommand.AddedToQueue, 
                        track.Title, track.Author, track.Length.ToString(@"hh\:mm\:ss"))
                });
            return;
        }

        StringBuilder stringBuilder;
            
        if (isSlavic)
        {
            var parts = language.SlavicParts;
            stringBuilder = new StringBuilder(string.Format(language.PlayCommand.PlaylistAddedToQueue,
                loadResult.LavalinkLoadResult.Tracks.Count, 
                translator.WordForSlavicLanguage(loadResult.LavalinkLoadResult.Tracks.Count, parts.OneTrack, parts.TwoTracks, parts.FiveTracks),
                loadResult.LavalinkLoadResult.PlaylistInfo.Name));
        }
        else
        {
            stringBuilder = new StringBuilder(string.Format(language.PlayCommand.PlaylistAddedToQueue,
                loadResult.LavalinkLoadResult.Tracks.Count, loadResult.LavalinkLoadResult.PlaylistInfo.Name));
        }
        
        foreach (var content in loadResult.LavalinkLoadResult.Tracks.Select(loadResultTrack => string.Format(language.PlayCommand.AddedToQueueMessagePattern, loadResultTrack.Title, loadResultTrack.Length.ToString(@"hh\:mm\:ss"), loadResultTrack.Author)).Where(content => stringBuilder.Length + content.Length <= 2000))
        {
            stringBuilder.Append(content);
        }
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = stringBuilder.ToString()
            });
    }
}
