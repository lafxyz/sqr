using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using SQR.Attributes;
using SQR.Exceptions;
using SQR.Extenstions;
using SQR.Services;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("vote-skip", "Start vote for skip or contribute active one")]
    [RequireLavalinkConnection]
    [BotAndUserMustBeInTheSameVoiceChannel]
    [UserMustBeInVoiceChannel]
    public async Task VoteSkipCommand(InteractionContext context)
    {
        var language = Language.GetLanguageOrFallback(_translator, context.Locale);

        var voiceState = context.Member.VoiceState;
        var conn = GetConnection(context);
        var connectedGuild = await _queue.GetConnectedGuild(context);
        var users = voiceState.Channel.Users.Where(x => x.IsBot == false).ToList();
        var threshold = (int)(users.Count * 0.7f);

        if (connectedGuild.NowPlaying is null)
            throw new CurrentTrackIsNullException();
        
        var voteSkip = connectedGuild.VoteSkip;
        if (voteSkip.RequestedTrackToSkip != connectedGuild.NowPlaying)
        {
            voteSkip.Reset();
            voteSkip.RequestedTrackToSkip = connectedGuild.NowPlaying;
        }

        if (voteSkip.Vote(context.User) == false)
            throw new AlreadyVotedException(connectedGuild.NowPlaying);

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(context.Client, language);

        var llt = connectedGuild.NowPlaying.LavalinkTrack;
        if (voteSkip.Users.Count >= threshold)
        {
            embed.WithDescription(string.Format(language.MusicTranslation.VoteSkipCommandTranslation.ThresholdPassed,
                llt.Title, llt.Author));
            
            if (connectedGuild.Looping == QueueService.LoopingState.Single
                && connectedGuild.NowPlaying == connectedGuild.Queue.ElementAt(0))
            {
                connectedGuild.Queue.RemoveAt(0);
            }

            await conn.StopAsync();

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(embed));
            return;
        }

        var required = threshold - voteSkip.Users.Count;
        
        if (language.IsSlavicLanguage)
        {
            var slavicParts = language.MusicTranslation.VoteSkipCommandTranslation.SlavicParts;
            embed.WithDescription(string.Format(language.MusicTranslation.VoteSkipCommandTranslation.Voted,
                llt.Title, llt.Author, required,
                Translator.WordForSlavicLanguage(required, slavicParts.OneVote,
                    slavicParts.TwoVotes, slavicParts.FiveVotes)
                ));
        }
        else
        {
            embed.WithDescription(string.Format(language.MusicTranslation.VoteSkipCommandTranslation.Voted,
                llt.Title, llt.Author, required));
        }

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embed));
    }
}