using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Lavalink;
using SQR.Expections;
using SQR.Extenstions;
using SQR.Services;
using SQR.Translation;
using SQR.Utilities;
using SQR.Workers;

namespace SQR.Commands.Music;
public partial class Music : ApplicationCommandsModule
{
    public Music(DatabaseService dbService, QueueWorker queue, Translator translator)
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
    private QueueWorker _queue { get; }
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
    
    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext context)
    {
        var voiceState = context.Member.VoiceState;

        var conn = GetConnection(context);

        if (voiceState is null)
        {
            if (_excludeVoiceState.Contains(context.CommandName) == false)
                throw new NotInVoiceChannelException();
        }

        if (conn == null)
        {
            if (_excludeIsConnected.Contains(context.CommandName) == false)
                throw new ClientIsNotConnectedException();
        }
        

        if (context.Guild.CurrentMember.VoiceState != null 
            && voiceState?.Channel != context.Guild.CurrentMember.VoiceState.Channel)
        {
            if (_excludeDifferentChannel.Contains(context.CommandName) == false)
                throw new DifferentVoiceChannelException();
        }

        return Task.FromResult(true);
    }

    private LavalinkGuildConnection? GetConnection(BaseContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        return node.GetGuildConnection(context.Guild);
    }
}