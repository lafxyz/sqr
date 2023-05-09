using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using SQR.Exceptions;

namespace SQR.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class UserMustBeInVoiceChannelAttribute : ApplicationCommandCheckBaseAttribute
{
    public override Task<bool> ExecuteChecksAsync(BaseContext context)
    {
        var voiceState = context.Member.VoiceState;
        
        if (voiceState is null || context.Guild.Id != voiceState.Guild.Id)
        {
            throw new NotInVoiceChannelException();
        }

        return Task.FromResult(true);
    }
}