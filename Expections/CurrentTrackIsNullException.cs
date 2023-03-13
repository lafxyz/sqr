namespace SQR.Expections;

public class CurrentTrackIsNullException : BaseException
{
    public CurrentTrackIsNullException(bool isDeferred = false) : base(isDeferred)
    {
    }

    public CurrentTrackIsNullException(string message, bool isDeferred = false) : base(message, isDeferred)
    {
    }
}