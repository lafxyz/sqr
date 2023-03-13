using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Expections;
using SQR.Models.Music;
using SQR.Services;
using SQR.Translation;
using SQR.Workers;

namespace SQR.Commands.Music;
public partial class Music : ApplicationCommandsModule
{
    public Music(DatabaseService dbService, QueueWorker queue, Translator translator)
    {
        DbService = dbService;
        Queue = queue;
        Translator = translator;
    }
    
    private Translator Translator { get; }
    private QueueWorker Queue { get; }
    private DatabaseService DbService { get; }

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
    
    public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext context)
    {
        var commandFull = context.CommandName + "command";
        var voiceState = context.Member.VoiceState;

        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);

        if (voiceState is null)
        {
            var exclude = new[]
            {
                nameof(PopularTracksCommand).ToLower()
            };
            
            if (exclude.Contains(commandFull) == false)
                throw new NotInVoiceChannelException();
        }

        if (conn is null)
        {
            var exclude = new[]
            {
                nameof(PlayCommand).ToLower(),
                nameof(PopularTracksCommand).ToLower()
            };
            if (exclude.Contains(commandFull) == false)
                throw new ClientIsNotConnectedException();
        }
        

        if (context.Guild.CurrentMember.VoiceState != null && voiceState?.Channel != context.Guild.CurrentMember.VoiceState.Channel)
        {
            var exclude = new[]
            {
                nameof(PopularTracksCommand).ToLower()
            };
            if (exclude.Contains(commandFull) == false)
                throw new DifferentVoiceChannelException();
        }

        return true;
    }
}