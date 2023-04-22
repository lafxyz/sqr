using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;

namespace SQR.Commands.Dev;

public partial class Dev
{
    [SlashCommand("paginated-modals", "Paginated modals!")]
    public async Task PaginatedModals(InteractionContext ctx)
    {
        _ = Task.Run(async () =>
        {
            var responses = await ctx.Interaction.CreatePaginatedModalResponseAsync(
                new List<ModalPage>()
                {
                    new ModalPage(new DiscordInteractionModalBuilder().WithTitle("First Title")
                        .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Small, "title", "Title", "Name", 0, 250, false))),
                    new ModalPage(new DiscordInteractionModalBuilder().WithTitle("Second Title")
                        .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Small, "title1", "Next Modal", "Some value here"))
                        .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "description1", "Some bigger thing here", required: false))),
                    new ModalPage(new DiscordInteractionModalBuilder().WithTitle("Third Title")
                        .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Small, "title2", "Title2", "Even more here", 0, 250, false))
                        .AddModalComponents(new DiscordTextComponent(TextComponentStyle.Paragraph, "description2", "and stuff here", required: false))),
                });

            // If the user didn't submit all modals, TimedOut will be true. We return the command as there is nothing to handle.
            if (responses.TimedOut)
                return;

            // We simply throw all response into the Console, you can do whatever with this.
            foreach (var b in responses.Responses)
                Console.WriteLine(b.ToString());

            // We use EditOriginalResponseAsync here because CreatePaginatedModalResponseAsync responds to the last modal with a thinking state.
            await responses.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent("Success"));
        });
    }
}