namespace SQR.Expections;

public class ClientIsNotConnectedException : BaseException
{
    public ClientIsNotConnectedException(bool isDeferred = false) : base(isDeferred)
    {
    }

    public ClientIsNotConnectedException(string message, bool isDeferred = false) : base(message, isDeferred)
    {
    }
}