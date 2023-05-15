using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using SQR.Commands;
using SQR.Commands.Music;
using SQR.Translation;
using SQR.Utilities;

namespace SQR.Extenstions;

public static class DiscordEmbedBuilderExtensions
{
    public static DiscordEmbedBuilder AddEmptyField(this DiscordEmbedBuilder builder, bool inline = false)
    {
        builder.AddField(inline ? EmbedUtilities.EmptyEmbedFiledInline : EmbedUtilities.EmptyEmbedFiled);
        return builder;
    }
    
    public static DiscordEmbedBuilder AsSQRDefault(this DiscordEmbedBuilder builder,
        DiscordClient client, 
        Language language)
    {
        var dev = Bot.Config.DeveloperUser;
        var infoCommand = client.GetApplicationCommands().GetGlobalCommand(typeof(Info), nameof(Info.InfoCommand))!.Mention;
        var footer = string.Format(language.Temporary.Footer, infoCommand);

        builder.Color = EmbedUtilities.DiscordBackgroundColor;
        builder.WithFooter(footer, client.CurrentUser.AvatarUrl);
        builder.WithTimestamp(DateTimeOffset.UtcNow);

        return builder; 
    }

    public static DiscordEmbedBuilder AsException(this DiscordEmbedBuilder builder,
                                                    Language lang, string description)
    {
        builder.Title = lang.Exceptions.SomethingWentWrong;
        builder.Description = description;
        builder.Color = DiscordColor.IndianRed;

        return builder;
    }
}