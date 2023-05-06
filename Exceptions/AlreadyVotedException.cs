using SQR.Models.Music;

namespace SQR.Exceptions;

public class AlreadyVotedException : BaseException
{
    public bool IsDeferred { get; }
    public Track Track { get; }

    public AlreadyVotedException(Track track, bool isDeferred = false) : base(isDeferred)
    {
        Track = track;
    }

    public AlreadyVotedException(string message, Track track, bool isDeferred = false) : base(message, isDeferred)
    {
        Track = track;
    }
}