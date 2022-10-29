using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Common;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Translation;
using SQR.Workers;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("shuffle", "Shuffles queue")]
    public async Task ShuffleCommand(InteractionContext context)
    {
        var language = Translator.Languages[Translator.LanguageCode.EN].Music;

        if (Translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].Music;
        }

        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        if (conn == null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.General.LavalinkIsNotConnected
                });
            return;
        }

        Queue.Shuffle(context);
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = language.ShuffleCommand.Shuffled
            });
    }
}