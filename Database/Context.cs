using Microsoft.EntityFrameworkCore;
using SQR.Database.Music;

namespace SQR.Database;

public sealed class Context : DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
        Database.EnsureCreated();
    }
    public DbSet<TrackDb> Tracks { get; set; } = null!;
}