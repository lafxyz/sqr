using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Attributes;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("volume", "Sets track volume")]
    [RequireLavalinkConnection]
    [BotAndUserMustBeInTheSameVoiceChannel]
    [UserMustBeInVoiceChannel]
    public async Task VolumeCommand(InteractionContext context, [Option("percent", "From 0 to 100")] int scale)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).MusicTranslation;
        
        scale = Math.Clamp(scale, 0, 100);
        await _queue.SetVolume(context, scale);

        var a = ""; 
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = string.Format(music.VolumeCommandTranslation.VolumeUpdated, scale)
            });
    }
}