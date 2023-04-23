using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;

namespace SQR.Commands.Dev;

public partial class Dev
{
    [SlashCommand("paginated", "Paginated modals!")]
    public async Task Paginated(InteractionContext ctx)
    {
        _ = Task.Run(async () =>
        {
            var lines = new List<string>();
            var itemsPerPage = 2;
            for (var i = 65; i < 75; i++)
            {
                lines.Add(new string((char)i, 3));
            }

            var pages = lines.Count / itemsPerPage;

            var interactivity = ctx.Client.GetInteractivity();

            var button = new DiscordButtonComponent(ButtonStyle.Primary, Guid.NewGuid().ToString(), "Test");

            var message = new DiscordInteractionResponseBuilder()
                .AsEphemeral()
                .WithContent("Hallo!")
                .AddComponents(button);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
            var response = await ctx.Interaction.GetOriginalResponseAsync();

            var waitForButton = await interactivity.WaitForButtonAsync(response, new List<DiscordButtonComponent> { button });

            if (waitForButton.TimedOut)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("TimedOut"));
                return;
            }
            
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You pressed button wow"));

        });
    }
}