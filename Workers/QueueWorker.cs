using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Common;
using DisCatSharp.Lavalink;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SQR.Database;
using SQR.Database.Music;
using SQR.Models.Music;
using SQR.Services;
using SQR.Translation;

namespace SQR.Workers;

public class QueueWorker
{
    public static Dictionary<LavalinkGuildConnection, ConnectedGuild> Servers => _servers;

    private static readonly string[] PlaylistKeywords = { "/album/", "/playlist/", "/artist/", "list=" };
    
    private static Dictionary<LavalinkGuildConnection, ConnectedGuild> _servers = new();

    private readonly DatabaseService _dbService;

    public QueueWorker(DatabaseService dbService, Translator translator)
    {
        _dbService = dbService;

        var queue = new BackgroundTask(TimeSpan.FromSeconds(3));
        queue.Start(async () =>
        {
            foreach (var (connection, connectedGuild) in _servers)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var context = connectedGuild.Context;

                        var language = translator!.Languages[Translator.LanguageCode.EN].Music;
                        var isSlavic = translator.Languages[Translator.LanguageCode.EN].IsSlavicLanguage;

                        if (context?.Locale != null && translator.LocaleMap.ContainsKey(context.Locale))
                        {
                            language = translator.Languages[translator.LocaleMap[context.Locale]].Music;
                            isSlavic = translator.Languages[translator.LocaleMap[context.Locale]].IsSlavicLanguage;
                        }

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
                            if (context != null)
                            {
                                if (isSlavic)
                                {
                                    await context.Channel.SendMessageAsync(string.Format(
                                        language.PlayCommand.LeavingInNSeconds, seconds,
                                        translator.WordForSlavicLanguage(seconds,
                                            language.SlavicParts.OneSecond,
                                            language.SlavicParts.TwoSeconds,
                                            language.SlavicParts.FiveSeconds)));
                                }
                                else
                                {
                                    await context.Channel.SendMessageAsync(
                                        string.Format(language.PlayCommand.LeavingInNSeconds, seconds));
                                }
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
                                if (context != null)
                                    await context.Channel.SendMessageAsync(language.PlayCommand.EmptyQueue);
                            }
                        }
                        else
                        {
                            var toPlay = (connectedGuild.Queue ?? throw new Exception(nameof(connectedGuild.Queue))).First();
                            connectedGuild.Queue.Remove(toPlay);

                            if (connectedGuild.Looping == LoopingState.LoopTrack) connectedGuild.Queue.Insert(0, toPlay);
                            if (connectedGuild.Looping == LoopingState.LoopQueue) connectedGuild.Queue.Add(toPlay);

                            await connection.PlayAsync(toPlay.LavalinkTrack);
                            if (connectedGuild.IsFirstTrackReceived == false) connectedGuild.IsFirstTrackReceived = true;

                            if (context != null)
                                await context.Channel.SendMessageAsync(
                                    string.Format(language.PlayCommand.NowPlaying, toPlay.LavalinkTrack.Title,
                                        toPlay.LavalinkTrack.Author,
                                        toPlay.LavalinkTrack.Length.ToString(@"hh\:mm\:ss")) +
                                    $"{(connection.CurrentState.CurrentTrack?.SourceName == "spotify" ? language.PlayCommand.IfPlaybackStopped : "")}");
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
        });
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

        LavalinkLoadResult lavalinkLoadResult;

        lavalinkLoadResult = uri is not null
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
        if (tracks != null)
            _servers[connection].Queue = tracks.OrderBy(x => new SecureRandom().Next()).ToList();
    }

    public Task<ConnectedGuild> GetConnectedGuild(InteractionContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState?.Guild);

        return Task.FromResult(_servers[connection]);
    }
    
    
}