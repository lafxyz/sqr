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
    }

    private Translator _translator { get; }
    private QueueService _queue { get; }
    private DatabaseService _dbService { get; }

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