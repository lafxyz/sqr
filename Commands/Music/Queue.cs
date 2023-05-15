using System.Diagnostics;
using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using SQR.Attributes;
using SQR.Extenstions;
using SQR.Models.Music;
using SQR.Pagination;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("queue", "Display queue")]
    [RequireLavalinkConnection]
    [BotAndUserMustBeInTheSameVoiceChannel]
    [UserMustBeInVoiceChannel]
    public async Task QueueCommand(InteractionContext context)
    {
        _ = Task.Run(async () =>
        {
            var language = Language.GetLanguageOrFallback(_translator, context.Locale);
            var music = language.MusicTranslation;

            var conn = GetConnection(context)!;

            var currentTrack = conn.CurrentState.CurrentTrack;

            var stringBuilder = new StringBuilder();

            var connectedGuild = await _queue.GetConnectedGuild(context);

            var tracks = connectedGuild.Queue;

            TimeSpan estimatedPlaybackTime = default;

            estimatedPlaybackTime = tracks.Aggregate(estimatedPlaybackTime,
                (current, track) => current.Add(track.LavalinkTrack.Length));

            var embed = new DiscordEmbedBuilder()
                .AsSQRDefault(context.Client, language);

            if (language.IsSlavicLanguage)
            {
                var parts = music.SlavicParts;

                stringBuilder = new StringBuilder(string.Format(music.QueueCommandTranslation.NowPlaying,
                    currentTrack.Title,
                    currentTrack.Author,
                    conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss"),
                    currentTrack.Length.ToString(@"hh\:mm\:ss"),
                    tracks.Count,
                    estimatedPlaybackTime.ToString(@"hh\:mm\:ss"),
                    Translator.WordForSlavicLanguage(tracks.Count, parts.OneTrack, parts.TwoTracks,
                        parts.FiveTracks)
                ));
            }
            else
            {
                stringBuilder = new StringBuilder(string.Format(music.QueueCommandTranslation.NowPlaying,
                    currentTrack.Title, currentTrack.Author,
                    conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss"),
                    currentTrack.Length.ToString(@"hh\:mm\:ss"), tracks.Count,
                    estimatedPlaybackTime.ToString(@"hh\:mm\:ss")
                ));
            }
            
            #region Pagination
            
            var pageContainer = new PageContainer<Track>(tracks, 5);
            
            DiscordButtonComponent[] buttons = {
                new(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Previous page", true),
                new(ButtonStyle.Secondary, Guid.NewGuid().ToString(), "Next page", pageContainer.Pages.Count == 1)
            };

            var current = pageContainer.Current();
            var transformedTracks = current.Content.Select(x =>
            {
                var lavalinkTrack = x.LavalinkTrack;
                var transformed = string.Format(music.QueueCommandTranslation.QueueMessagePattern, lavalinkTrack.Author,
                    lavalinkTrack.Title, lavalinkTrack.Length.ToString(@"hh\:mm\:ss"), x.DiscordUser.Mention);
                return transformed;
            });

            var content = string.Join("\n", transformedTracks);
            var currentPage = string.Format(language.MusicTranslation.QueueCommandTranslation.CurrentPage,
                current.Index, pageContainer.Pages.Count);
            embed.WithDescription(stringBuilder + currentPage + content);
            
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                        .AddEmbed(embed)
                        .AddComponents(buttons));
            
            var response = await context.GetOriginalResponseAsync();
            var timeOutOverride = TimeSpan.FromMinutes(15);
            var expiresAt = DateTime.UtcNow.Add(timeOutOverride);
            var interactivity = context.Client.GetInteractivity();

            while (expiresAt > DateTime.UtcNow)
            {
                var interactivityResult = await interactivity.WaitForButtonAsync(response, buttons, timeOutOverride);
                
                if (interactivityResult.Result.User.Id != context.User.Id) continue;
                
                if (interactivityResult.TimedOut) break;
                
                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);

                Page<Track>? page = null;

                if (interactivityResult.Result.Id == buttons[0].CustomId) // Prev
                {
                    page = pageContainer.PreviousOrDefault();

                } else if (interactivityResult.Result.Id == buttons[1].CustomId) // Next
                {
                    page = pageContainer.NextOrDefault();
                }
                
                if (page is null) continue;
                if (page.Index == 1)
                    buttons[0].Disable();
                else
                    buttons[0].Enable();

                if (pageContainer.Pages.Count >= 2 && page.Index != pageContainer.Pages.Count)
                    buttons[1].Enable();
                else
                    buttons[1].Disable();

                transformedTracks = page.Content.Select(x =>
                {
                    var lavalinkTrack = x.LavalinkTrack;
                    var transformed = string.Format(music.QueueCommandTranslation.QueueMessagePattern, lavalinkTrack.Author,
                        lavalinkTrack.Title, lavalinkTrack.Length.ToString(@"hh\:mm\:ss"), x.DiscordUser.Mention);
                    return transformed;
                });
                
                content = string.Join("\n", transformedTracks);
                currentPage = string.Format(language.MusicTranslation.QueueCommandTranslation.CurrentPage,
                    page.Index, pageContainer.Pages.Count);

                embed.WithDescription(stringBuilder + currentPage + content);

                response = await context.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(embed)
                    .AddComponents(buttons));
            }
            
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent(string.Join("\n", string.Join("\n", pageContainer.Current().Content))));
            
            #endregion
        });
    }
}
