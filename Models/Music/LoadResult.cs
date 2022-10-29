using DisCatSharp.Lavalink;

namespace SQR.Models.Music;

public class LoadResult
{
    public LavalinkLoadResult LavalinkLoadResult => _lavalinkLoadResult;
    public bool IsPlaylist => _isPlaylist;
    
    private LavalinkLoadResult _lavalinkLoadResult;
    private bool _isPlaylist;

    public LoadResult(LavalinkLoadResult lavalinkLoadResult, bool isPlaylist)
    {
        _lavalinkLoadResult = lavalinkLoadResult;
        _isPlaylist = isPlaylist;
    }
}