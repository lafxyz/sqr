using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.BackgroundTasks;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("skip", "Skip current playing track")]
    public async Task SkipCommand(InteractionContext context)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).Music;

        var conn = GetConnection(context);

        var connectedGuild = await _queue.GetConnectedGuild(context);

        if (connectedGuild.Looping == QueueWorker.LoopingState.Single
            && connectedGuild.NowPlaying == connectedGuild.Queue.ElementAt(0))
        {
            Console.WriteLine("true");
            connectedGuild.Queue.RemoveAt(0);
        }

        await conn.StopAsync();
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = string.Format(music.SkipCommand.Skipped, conn.CurrentState.CurrentTrack.Title, conn.CurrentState.CurrentTrack.Author)
            });
    }
}