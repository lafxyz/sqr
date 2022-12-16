using SQR.Database.Music;

namespace SQR.Services;

public interface IDatabaseService
{
    Task<List<TrackDb>> GetTracksAsync();
    Task<TrackDb> GetTrackAsync(Guid trackId);
    Task<bool> CreateTrackAsync(TrackDb track);
}