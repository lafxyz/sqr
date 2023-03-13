namespace SQR.Expections;

public class NotInVoiceChannelException : BaseException
{
    public NotInVoiceChannelException(bool isDeferred = false) : base(isDeferred)
    {
    }

    public NotInVoiceChannelException(string message, bool isDeferred = false) : base(message, isDeferred)
    {
    }
}