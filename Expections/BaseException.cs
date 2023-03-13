namespace SQR.Expections;

public class BaseException : Exception
{
    public bool IsDeferred { get; }
    public BaseException(bool isDeferred = false)
    {
        IsDeferred = isDeferred;
    }
    
    public BaseException(string message, bool isDeferred = false) : base(message)
    {
        IsDeferred = isDeferred;
    }

}