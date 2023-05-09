using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using SQR.Exceptions;

namespace SQR.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class BotAndUserMustBeInTheSameVoiceChannelAttribute : ApplicationCommandCheckBaseAttribute
{
    public override Task<bool> ExecuteChecksAsync(BaseContext context)
    {
        var voiceState = context.Member.VoiceState;
        
        if (context.Guild.CurrentMember.VoiceState != null 
            && voiceState?.Channel != context.Guild.CurrentMember.VoiceState.Channel)
        {
            throw new DifferentVoiceChannelException();
        }

        return Task.FromResult(true);
    }
}