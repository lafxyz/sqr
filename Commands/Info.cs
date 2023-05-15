using System.Text;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Extenstions;
using SQR.Translation;

namespace SQR.Commands;

public class Info : ApplicationCommandsModule
{
    private Translator _translator;
    
    public Info(Translator translator)
    {
        _translator = translator;
    }

    [SlashCommand("info", "Info about why bot will not be maintained")]
    public async Task InfoCommand(InteractionContext context)
    {
        var language = Language.GetLanguageOrFallback(_translator, context.Locale);

        var stringBuilder = new StringBuilder();
        foreach (var line in language.Temporary.ArmyLines)
        {
            stringBuilder.AppendLine(line);
        }

        var embed = new DiscordEmbedBuilder();
        embed.AsSQRDefault(context.Client, language);
        embed.WithDescription(stringBuilder.ToString());

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(embed));
    }
}