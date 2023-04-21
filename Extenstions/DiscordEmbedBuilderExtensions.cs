using DisCatSharp.Entities;
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
    
    public static DiscordEmbedBuilder AsSQRDefault(this DiscordEmbedBuilder builder)
    {
        var dev = Bot.Config.DeveloperUser;

        builder.Color = EmbedUtilities.DiscordBackgroundColor;
        builder.WithFooter($"SQR by {dev.Username}#{dev.Discriminator}");
        builder.WithTimestamp(DateTimeOffset.UtcNow);

        return builder; 
    }

    public static DiscordEmbedBuilder AsException(this DiscordEmbedBuilder builder,
                                                    Language lang, string description)
    {
        builder.Title = "You ran into an exception!";
        builder.Description = description;
        builder.Color = DiscordColor.IndianRed;

        return builder;
    }
}