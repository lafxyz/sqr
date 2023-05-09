using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink;
using SQR.Exceptions;
using SQR.Extenstions;
using SQR.Services;
using SQR.Translation;
using SQR.Utilities;

namespace SQR.Commands.Music;
public partial class Music : ApplicationCommandsModule
{
    public Music(DatabaseService dbService, QueueService queue, Translator translator)
    {
        _dbService = dbService;
        _queue = queue;
        _translator = translator;
        
        SetupExcludes();
    }

    private void SetupExcludes()
    {
        var musicType = typeof(Music);
        _excludeVoiceState = new List<string>
        {
            AssemblyScanUtilities.GetSlashCommandAttribute(musicType, nameof(PopularTracksCommand)).Name
        };

        _excludeIsConnected = new List<string>
        {
            AssemblyScanUtilities.GetSlashCommandAttribute(musicType, nameof(PlayCommand)).Name,
            AssemblyScanUtilities.GetSlashCommandAttribute(musicType, nameof(PopularTracksCommand)).Name
        };

        _excludeDifferentChannel = new List<string>
        {
            AssemblyScanUtilities.GetSlashCommandAttribute(musicType, nameof(PopularTracksCommand)).Name
        };
    }

    private Translator _translator { get; }
    private QueueService _queue { get; }
    private DatabaseService _dbService { get; }

    private List<string> _excludeVoiceState;
    private List<string> _excludeIsConnected;
    private List<string> _excludeDifferentChannel;

    public enum EqPresets
    {
        [ChoiceName("EarRape")]
        EarRape,
        [ChoiceName("Bass")]
        Bass,
        [ChoiceName("Pop")]
        Pop,
        [ChoiceName("Default")]
        Default
    }

    public static LavalinkGuildConnection? GetConnection(BaseContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.FirstOrDefault();
        return node?.GetGuildConnection(context.Guild);
    }
}