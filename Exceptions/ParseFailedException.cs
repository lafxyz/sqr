namespace SQR.Exceptions;

public class ParseFailedException : BaseException
{
    public ParseFailedException(string message, bool isDeferred = false) : base(message, isDeferred)
    {
    }
}