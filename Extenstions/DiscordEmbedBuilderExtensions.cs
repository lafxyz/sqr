using DisCatSharp.Entities;
using SQR.Translation;

namespace SQR.Extenstions;

public static class DiscordEmbedBuilderExtensions
{
    public static DiscordEmbedBuilder AsSQRDefault(this DiscordEmbedBuilder builder)
    {
        var dev = Bot.Config.DeveloperUser;

        builder.Color = new DiscordColor("#2b2d31");
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