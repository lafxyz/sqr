namespace SQR.Expections;

public class LavalinkIsNotConnectedException : BaseException
{
    public LavalinkIsNotConnectedException(bool isDeferred = false) : base(isDeferred)
    {
    }

    public LavalinkIsNotConnectedException(string message, bool isDeferred = false) : base(message, isDeferred)
    {
    }
}