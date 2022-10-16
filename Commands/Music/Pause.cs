using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("pause", "Pauses playback")]
    public async Task PauseCommand(InteractionContext context)
    {
        var scope = context.Services.CreateScope();
        var translator = scope.ServiceProvider.GetService<Translator>();

        var isSlavic = translator.Languages[Translator.LanguageCode.EN].IsSlavicLanguage;

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

        if (context.Guild.CurrentMember.VoiceState != null && voiceState.Channel != context.Guild.CurrentMember.VoiceState.Channel)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.General.DifferentVoice
                });
            return;
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
        
        await conn.PauseAsync();

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = language.PauseCommand.Paused
            });
    }
}