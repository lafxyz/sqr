using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Attributes;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("shuffle", "Shuffles queue")]
    [RequireLavalinkConnection]
    [BotAndUserMustBeInTheSameVoiceChannel]
    [UserMustBeInVoiceChannel]
    public async Task ShuffleCommand(InteractionContext context)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).MusicTranslation;
    
        _queue.Shuffle(context);
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = music.ShuffleCommandTranslation.Shuffled
            });
    }
}