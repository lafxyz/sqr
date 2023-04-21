using System.ComponentModel.DataAnnotations;

namespace SQR.Database.Music;

public class TrackDb
{
    [Key]
    public Guid Id { set; get; } = Guid.NewGuid();
    public string Name { set; get; } = null!;
    public string Author { set; get; } = null!;
    public ulong DiscordUserId { set; get; }
}