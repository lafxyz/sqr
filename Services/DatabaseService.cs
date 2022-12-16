using Microsoft.EntityFrameworkCore;
using SQR.Database;
using SQR.Database.Music;

namespace SQR.Services;

public class DatabaseService : IDatabaseService
{
    private Context _dbContext;

    public DatabaseService(Context dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TrackDb>> GetTracksAsync()
    {
        return await _dbContext.Tracks.ToListAsync();
    }

    public async Task<TrackDb> GetTrackAsync(Guid trackId)
    {
        return await _dbContext.Tracks.SingleOrDefaultAsync(db => db.Id == trackId) ?? 
               throw new InvalidOperationException($"{trackId} doesn't exists in this database context");
    }

    public async Task<bool> CreateTrackAsync(TrackDb track)
    {
        await _dbContext.Tracks.AddAsync(track);
        var created = await _dbContext.SaveChangesAsync();
        return created > 0;
    }
    
    
}