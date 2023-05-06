using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Translation;

namespace SQR.Commands.Dev;

public partial class Dev
{

    [SlashCommand("reloadlanguages", "Reloads phrases for all languages")]
    public async Task ReloadLanguagesCommand(InteractionContext context)
    {
        var language = Translator.Languages[Translator.LanguageCode.EN].DevTranslation;

        if (Translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].DevTranslation;
        }
        
        if (!context.Client.CurrentApplication.Owners.Contains(context.User))
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.GeneralTranslation.NoAccess
                });
            return; 
        }

        try
        {
            Translator.Reload();
        
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.ReloadCommandTranslation.Reloaded
                });
        }
        catch (Exception e)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = e.ToString()
                });
            throw;
        }
    }
}