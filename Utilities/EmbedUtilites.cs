using DisCatSharp.Entities;

namespace SQR.Utilities;

public class EmbedUtilites
{
    public static readonly DiscordEmbedField EmptyEmbedFiled = new("‎ ", "‎ ");
    public static readonly DiscordEmbedField EmptyEmbedFiledInline = new("‎ ", "‎ ", true);
}