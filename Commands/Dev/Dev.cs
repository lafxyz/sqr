using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using SQR.Translation;

namespace SQR.Commands.Dev;

[SlashCommandGroup("dev", "Yes yes yes")]
public partial class Dev : ApplicationCommandsModule
{
#pragma warning disable CS8618
    public Translator Translator { private get; set; }
#pragma warning restore CS8618
}