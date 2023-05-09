using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using SQR.Commands.Music;
using SQR.Exceptions;

namespace SQR.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class RequireLavalinkConnectionAttribute : ApplicationCommandCheckBaseAttribute
{
    public override Task<bool> ExecuteChecksAsync(BaseContext context)
    {
        var conn = Music.GetConnection(context);
        
        if (conn == null)
        {
            throw new ClientIsNotConnectedException();
        }

        return Task.FromResult(true);
    }
}