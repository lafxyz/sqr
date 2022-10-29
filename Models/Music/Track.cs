using DisCatSharp.Entities;
using DisCatSharp.Lavalink;

namespace SQR.Models.Music;

public class Track
{
    public LavalinkTrack LavalinkTrack => _lavalinkTrack;
    public DiscordUser DiscordUser => _discordUser;

    private LavalinkTrack _lavalinkTrack;
    private DiscordUser _discordUser;
    
    public Track(LavalinkTrack lavalinkTrack, DiscordUser discordUser)
    {
        _lavalinkTrack = lavalinkTrack;
        _discordUser = discordUser;
    }
}