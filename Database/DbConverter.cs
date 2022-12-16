using SQR.Database.Music;
using SQR.Models.Music;
using SQR.Workers;

namespace SQR;

public static class DbConverter
{
    public static IEnumerable<TrackDb> ToListOfTrackDb(IEnumerable<Track> queue)
    {
        return queue.Select(track => new TrackDb
        {
            Name = track.LavalinkTrack.Title,
            Author = track.LavalinkTrack.Author,
            DiscordUserId = track.DiscordUser.Id
        });
    }
    
    public static TrackDb ToTrackDb(Track track)
    {
        return new TrackDb
        {
            Name = track.LavalinkTrack.Title,
            Author = track.LavalinkTrack.Author,
            DiscordUserId = track.DiscordUser.Id
        };
    }
}