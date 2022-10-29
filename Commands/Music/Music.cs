using DisCatSharp.ApplicationCommands;
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

[SlashCommandGroup("Music", "Music module")]
public partial class Music : ApplicationCommandsModule
{
#pragma warning disable CS8618
    public Translator Translator { private get; set; }
    public QueueWorker Queue { private get; set; }
#pragma warning restore CS8618
    
    public enum EqPresets
    {
        [ChoiceName("EarRape")]
        EarRape,
        [ChoiceName("Bass")]
        Bass,
        [ChoiceName("Pop")]
        Pop,
        [ChoiceName("Default")]
        Default
    }
    
    public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext context)
    {

        var language = Translator.Languages[Translator.LanguageCode.EN].Music;

        if (Translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].Music;
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
            return await base.BeforeSlashExecutionAsync(context);
        }

        return await base.BeforeSlashExecutionAsync(context);
    }
}