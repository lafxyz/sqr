using DisCatSharp.ApplicationCommands.Attributes;

namespace SQR.Utilities;

public static class AssemblyScanUtilities
{
    public static SlashCommandAttribute GetSlashCommandAttribute(Type type, string command)
    {
        var attributes = type.GetMethod(command)?.GetCustomAttributes(false);
        
        return (SlashCommandAttribute)attributes!.First(x => x is SlashCommandAttribute);
    }
}