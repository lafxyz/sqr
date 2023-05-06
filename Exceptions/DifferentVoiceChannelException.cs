namespace SQR.Exceptions;

public class DifferentVoiceChannelException : BaseException
{
    public DifferentVoiceChannelException(bool isDeferred = false) : base(isDeferred)
    {
    }

    public DifferentVoiceChannelException(string message, bool isDeferred = false) : base(message, isDeferred)
    {
    }
}