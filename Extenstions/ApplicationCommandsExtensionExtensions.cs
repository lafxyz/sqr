using System.Xml.Schema;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.Entities;
using SQR.Commands.Music;
using SQR.Utilities;

namespace SQR.Extenstions;

public static class ApplicationCommandsExtensionExtensions
{
    public static DiscordApplicationCommand? GetGlobalCommand(this ApplicationCommandsExtension extension, 
        Type type,
        string commandName)
    {
        return extension.GlobalCommands.FirstOrDefault(x =>
            x.Name == AssemblyScanUtilities.GetSlashCommandAttribute(type, commandName).Name
        );
    }
    
    public static DiscordApplicationCommand? GetGuildCommand(this ApplicationCommandsExtension extension,
        Type type,
        ulong guildId, string commandName)
    {
        return extension.GuildCommands[guildId].FirstOrDefault(x =>
            x.Name == AssemblyScanUtilities.GetSlashCommandAttribute(type, commandName).Name
        );
    }
}