using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Expections;
using SQR.Translation;
using SQR.Workers;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("loop", "Defines loop mode")]
    public async Task LoopCommand(InteractionContext context, [Option("mode", "Looping mode")] QueueWorker.LoopingState state)
    {
        var language = Translator.Languages[Translator.FallbackLanguage].Music;

        if (Translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].Music;
        }

        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState?.Guild);

        Queue.SetLoopState(context, state);

        var map = new Dictionary<QueueWorker.LoopingState, string>
        {
            { QueueWorker.LoopingState.NoLoop, language.LoopCommand.NoLoop },
            { QueueWorker.LoopingState.LoopTrack, language.LoopCommand.LoopTrack },
            { QueueWorker.LoopingState.LoopQueue, language.LoopCommand.LoopQueue }
        };

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = map[state]
            });
    }
}