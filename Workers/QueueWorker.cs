using System.ComponentModel;
using DisCatSharp;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Common;
using DisCatSharp.EventArgs;
using DisCatSharp.Lavalink;
using SQR.Commands.Music;
using SQR.Database;
using SQR.Models.Music;
using SQR.Services;
using SQR.Translation;

namespace SQR.Workers;

public class QueueWorker
{
    public static Dictionary<LavalinkGuildConnection, ConnectedGuild> Servers => _servers;

    private static readonly string[] PlaylistKeywords = { "/album/", "/playlist/", "/artist/", "list=" };
    
    private static readonly Dictionary<LavalinkGuildConnection, ConnectedGuild> _servers = new();

    private readonly DatabaseService _dbService;
    private readonly Translator _translator;

    public QueueWorker(DatabaseService dbService, Translator translator)
    {
        _dbService = dbService;
        _translator = translator;

        var queue = new BackgroundTask(TimeSpan.FromSeconds(3));
        
        queue.AssignAndStartTask(async () =>
        {
            foreach (var (connection, connectedGuild) in _servers)
            {
                try
                {
                    await ProcessQueue(connection, connectedGuild);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        });
    }

    private async Task ProcessQueue(LavalinkGuildConnection connection, ConnectedGuild connectedGuild)
    {
        var context = connectedGuild.Context;

        var language = Language.GetLanguageOrFallback(_translator, context.Locale);
        var music = language.Music;
        if (connection.IsConnected == false)
        {
            await DisconnectAsync(connection);
            return;
        }

        if (connection.CurrentState.CurrentTrack != null)
        {
            return;
        }

        if (connectedGuild.Queue.Any() == false)
        {
            if (connectedGuild.IsFirstTrackReceived == false) return;
            if (connectedGuild.WaitingForTracks) return;
            
            const int seconds = 60;
            if (language.IsSlavicLanguage)
            {
                //TODO: embed
                await context.Channel.SendMessageAsync(string.Format(
                    music.QueueWorker.LeavingInNSeconds, seconds,
                    Translator.WordForSlavicLanguage(seconds,
                        music.SlavicParts.OneSecond,
                        music.SlavicParts.TwoSeconds,
                        music.SlavicParts.FiveSeconds)));
            }
            else
            {
                //TODO: embed
                await context.Channel.SendMessageAsync(
                    string.Format(music.QueueWorker.LeavingInNSeconds, seconds));
            }

            connectedGuild.WaitingForTracks = true;
            connectedGuild.WaitingForTracksSince = DateTimeOffset.Now;

            while (DateTimeOffset.Now - connectedGuild.WaitingForTracksSince < TimeSpan.FromSeconds(seconds))
            {
                if (connectedGuild.WaitingForTracks == false)
                {
                    return;
                }
                await Task.Delay(1000);
            }

            if (connectedGuild.Queue.Any() == false)
            {
                await DisconnectAsync(connection);
                //TODO: embed
                await context.Channel.SendMessageAsync(music.QueueWorker.EmptyQueue);
            }
        }
        else
        {
            var toPlay = connectedGuild.Queue.First();
            
            //TODO: Fix loop edge-case (skip command)
            if (connectedGuild.IsSkipRequested == false)
            {
                if (connectedGuild.Looping != LoopingState.LoopTrack) connectedGuild.Queue.Remove(toPlay);
                if (connectedGuild.Looping == LoopingState.LoopQueue) connectedGuild.Queue.Add(toPlay);
                connectedGuild.IsSkipRequested = false;
            }

            await connection.PlayAsync(toPlay.LavalinkTrack);
            if (connectedGuild.IsFirstTrackReceived == false) 
                connectedGuild.IsFirstTrackReceived = true;

            //TODO: embed
            await context.Channel.SendMessageAsync(
                string.Format(music.QueueWorker.NowPlaying, toPlay.LavalinkTrack.Title,
                    toPlay.LavalinkTrack.Author,
                    toPlay.LavalinkTrack.Length.ToString(@"hh\:mm\:ss")) +
                $"{(connection.CurrentState.CurrentTrack!.SourceName == "spotify" ? music.QueueWorker.IfPlaybackStopped : "")}");
            await Task.Delay(1000);
        }
    }

    public enum SearchSources
    {
        [ChoiceName("YouTube")]
        YouTube,
        [ChoiceName("Spotify")]
        Spotify,
        [ChoiceName("AppleMusic")]
        AppleMusic,
        [ChoiceName("SoundCloud")]
        SoundCloud
    }
    
    public enum LoopingState
    {
        [ChoiceName("Without looping")]
        NoLoop,
        [ChoiceName("Loop current track")]
        LoopTrack,
        [ChoiceName("Loop current queue")]
        LoopQueue
    }

    public static async Task VoiceStateUpdate(DiscordClient sender, VoiceStateUpdateEventArgs e)
    {
        if (e.User == sender.CurrentUser && e.After.Channel == null)
        {
            var lava = sender.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var connection = node.GetGuildConnection(e.Guild);

            if (_servers.ContainsKey(connection))
            {
                await connection.DisconnectAsync();
                _servers.Remove(connection);
            }
        }
    }

    public async Task<LoadResult> AddAsync(InteractionContext context, string search, SearchSources source)
    {
        Uri.TryCreate(search, UriKind.Absolute, out var uri);

        var searchMap = new Dictionary<SearchSources, LavalinkSearchType>
        {
            { SearchSources.YouTube, LavalinkSearchType.Youtube },
            { SearchSources.AppleMusic, LavalinkSearchType.AppleMusic },
            { SearchSources.Spotify, LavalinkSearchType.Spotify },
            { SearchSources.SoundCloud, LavalinkSearchType.SoundCloud }
        };

        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Guild);

        var lavalinkLoadResult = uri is not null
            ? await node.Rest.GetTracksAsync(uri)
            : await node.Rest.GetTracksAsync(search, searchMap[source]);

        if (lavalinkLoadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            var tracks = Servers[connection].Queue;
            if (tracks.Any() == false) await DisconnectAsync(connection);
        }

        if (!PlaylistKeywords.Any(search.Contains))
        {
            var lavalinkTrack = lavalinkLoadResult.Tracks.First();
            var tracks = _servers[connection].Queue;
            
            var track = new Track(lavalinkTrack, context.User);
            
            await _dbService.CreateTrackAsync(DbConverter.ToTrackDb(track));

            tracks.Add(track);

            if (_servers[connection].IsFirstTrackReceived == false)
            {
                _servers[connection].IsFirstTrackReceived = true;
            }
                    
            if (_servers[connection].WaitingForTracks)
            {
                _servers[connection].WaitingForTracks = false;
            }
                    
            return new LoadResult(lavalinkLoadResult, false);
        }
        
        foreach (var loadResultTrack in lavalinkLoadResult.Tracks)
        {
            var tracks = _servers[connection].Queue;
            var track = new Track(loadResultTrack, context.User);
            
            tracks.Add(track);
            
            await _dbService.CreateTrackAsync(DbConverter.ToTrackDb(track));
        }
        
        _servers[connection].IsFirstTrackReceived = true;

        return new LoadResult(lavalinkLoadResult, true);
    }
    
    public async Task<bool> TryConnectAsync(InteractionContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);

        if (connection is not null) return false;
        
        connection = await node.ConnectAsync(context.Member.VoiceState?.Channel);
        if (_servers.ContainsKey(connection)) return true;
            
        var connectedGuild = new ConnectedGuild
        {
            Looping = LoopingState.NoLoop,
            Queue = new List<Track>(),
            Context = context,
            Volume = 100
        };
        _servers.Add(connection, connectedGuild);
        return true;
    }

    public async Task DisconnectAsync(LavalinkGuildConnection connection)
    {
        _servers.Remove(connection);

        if (connection.IsConnected)
            await connection.DisconnectAsync();
    }

    public async Task SetVolume(InteractionContext context, int scale)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        await connection.SetVolumeAsync(scale);
        _servers[connection].Volume = scale;
    }
    
    public void SetLoopState(InteractionContext context, LoopingState state)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        _servers[connection].Looping = state;
    }

    public void Shuffle(InteractionContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);

        var tracks = _servers[connection].Queue;
        _servers[connection].Queue = tracks.OrderBy(x => new SecureRandom().Next()).ToList();
    }
    
    public async Task PauseAsync(InteractionContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);

        await connection.PauseAsync();
        _servers[connection].IsPaused = true;
    }
    
    public async Task ResumeAsync(InteractionContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);

        await connection.ResumeAsync();
        _servers[connection].IsPaused = false;
    }

    public void SetPreset(InteractionContext context, Music.EqPresets preset)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        _servers[connection].Preset = preset;
    }

    public Task<ConnectedGuild> GetConnectedGuild(InteractionContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState?.Guild);

        return Task.FromResult(_servers[connection]);
    }
    
    public Task<ConnectedGuild> GetConnectedGuild(LavalinkGuildConnection connection)
    {
        return Task.FromResult(_servers[connection]);
    }

}