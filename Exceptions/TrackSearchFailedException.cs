namespace SQR.Exceptions;

public class TrackSearchFailedException : BaseException
{
    public bool IsDeferred { get; }
    public string SearchString { get; }

    public TrackSearchFailedException(string searchString, bool isDeferred = false) : base(isDeferred)
    {
        SearchString = searchString;
    }

    public TrackSearchFailedException(string message, string searchString, bool isDeferred = false) : base(message, isDeferred)
    {
        SearchString = searchString;
    }
}