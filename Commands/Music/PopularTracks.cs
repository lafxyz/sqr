using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Database.Music;
using SQR.Translation;
using TimeSpanParserUtil;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("populartracks", "Returns list of most played tracks in the bot")]
    public async Task PopularTracksCommand(InteractionContext context)
    {
        await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder()
        {
            IsEphemeral = true
        });
        
        var language = Translator.Languages[Translator.FallbackLanguage].Music;

        if (Translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].Music;
        }
        
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);

        var tracks = await DbService.GetTracksAsync();

        var unsorted = new Dictionary<TrackDb, int>();
        
        foreach (var track in tracks)
        {
            if (unsorted.ContainsKey(track ?? throw new InvalidOperationException()))
            {
                unsorted[track] += 1;
            }
            else
            {
                unsorted.Add(track, 1);
            }
        }

        var top = unsorted.OrderByDescending(x => x.Value).Take(10);

        var stringbuilder = new StringBuilder();

        foreach (var (track, i) in top)
        {
            stringbuilder.AppendLine($"{track.Name} by {track.Author} has been played {i} times");
        }
        
        await context.EditResponseAsync(new DiscordWebhookBuilder()
            {
                Content = stringbuilder.ToString()
            });
    }
}