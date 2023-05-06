using System.Text;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Serilog;
using SQR.Extenstions;
using SQR.Translation;

namespace SQR.Exceptions;

public class ExceptionHandler
{
    private readonly Translator _translator;
    
    public ExceptionHandler(Translator translator)
    {
        _translator = translator;
    }

    public async Task HandleSlashException(ApplicationCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        var language = _translator.Languages[Translator.FallbackLanguage];
        if (_translator.LocaleMap.TryGetValue(e.Context.Locale, out var value))
        { 
            language = _translator.Languages[value];
        }
        
        if (await HandleLavaLinkIsNotConnected(e, language)) return;
        if (await HandleNotInVoiceChannel(e, language)) return;
        if (await HandleClientIsNotConnected(e, language)) return;
        if (await HandleDifferentVoiceChannel(e, language)) return;
        if (await HandleTrackSearchFailed(e, language)) return;
        if (await HandleCurrentTrackIsNull(e, language)) return;
        if (await HandleAlreadyVoted(e, language)) return;
        if (await HandleParseFailed(e, language)) return;

        await UnhandledExceptionReceived(e, language);
    }

    private async Task UnhandledExceptionReceived(SlashCommandErrorEventArgs e, Language lang)
    {
        Log.Logger.Warning("SUPPRESSED EXCEPTION!\n {Exception}", e.Exception);

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(e.Context.Client)
            .AsException(lang, "```" + e.Exception + "```");

        var baseException = e.Exception as BaseException;
        var isDeferred = baseException?.IsDeferred ?? false;

        await SendOrEditResponseAsync(e.Context, embed, isDeferred);
    }

    private async Task<bool> HandleLavaLinkIsNotConnected(SlashCommandErrorEventArgs e, Language lang)
    {
        if (e.Exception is not LavalinkIsNotConnectedException exception) return false;

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(e.Context.Client)
            .AsException(lang, lang.Exceptions.LavalinkIsNotConnected);
        
        await SendOrEditResponseAsync(e.Context, embed, exception.IsDeferred);
        return true;
    }
    
    private async Task<bool> HandleAlreadyVoted(SlashCommandErrorEventArgs e, Language lang)
    {
        if (e.Exception is not AlreadyVotedException exception) return false;

        var llt = exception.Track.LavalinkTrack;
        
        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(e.Context.Client)
            .AsException(lang, string.Format(
                    lang.Exceptions.AlreadyVoted, llt.Title, llt.Author
                ));
        
        await SendOrEditResponseAsync(e.Context, embed, exception.IsDeferred);
        return true;
    }
    
    private async Task<bool> HandleParseFailed(SlashCommandErrorEventArgs e, Language lang)
    {
        if (e.Exception is not ParseFailedException exception) return false;

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(e.Context.Client)
            .AsException(lang, string.Format(lang.Exceptions.ParseFailed, e.Exception.Message));

        await SendOrEditResponseAsync(e.Context, embed, exception.IsDeferred);

        return true;
    }

    private async Task<bool> HandleNotInVoiceChannel(SlashCommandErrorEventArgs e, Language lang)
    {
        if (e.Exception is not NotInVoiceChannelException exception) return false;
        
        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(e.Context.Client)
            .AsException(lang, lang.Exceptions.NotInVoice);
        
        await SendOrEditResponseAsync(e.Context, embed, exception.IsDeferred);
        return true;

    }

    private async Task<bool> HandleClientIsNotConnected(SlashCommandErrorEventArgs e, Language lang)
    {
        if (e.Exception is not ClientIsNotConnectedException exception) return false;

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(e.Context.Client)
            .AsException(lang, lang.Exceptions.ClientIsNotConnected);

        await SendOrEditResponseAsync(e.Context, embed, exception.IsDeferred);

        return true;
    }
    
    private async Task<bool> HandleDifferentVoiceChannel(SlashCommandErrorEventArgs e, Language lang)
    {
        if (e.Exception is not DifferentVoiceChannelException exception) return false;
        
        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(e.Context.Client)
            .AsException(lang, lang.Exceptions.DifferentVoice);
        
        await SendOrEditResponseAsync(e.Context, embed, exception.IsDeferred);
        
        
        return true;
    }
    
    private async Task<bool> HandleTrackSearchFailed(SlashCommandErrorEventArgs e, Language lang)
    {
        if (e.Exception is not TrackSearchFailedException exception) return false;

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(e.Context.Client)
            .AsException(lang, string.Format(lang.Exceptions.TrackSearchFailed, 
                exception.SearchString)
            );
        
        await SendOrEditResponseAsync(e.Context, embed, exception.IsDeferred);
        
        return true;
    }
    
    private async Task<bool> HandleCurrentTrackIsNull(SlashCommandErrorEventArgs e, Language lang)
    {
        if (e.Exception is not CurrentTrackIsNullException exception) return false;

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(e.Context.Client)
            .AsException(lang, lang.Exceptions.NothingIsPlaying);

        await SendOrEditResponseAsync(e.Context, embed, exception.IsDeferred);

        return true;
    }

    private async Task SendOrEditResponseAsync(BaseContext context, DiscordEmbed embed, bool isDeferred)
    {
        if (isDeferred) {
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(embed));
        }
        else
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
                new DiscordInteractionResponseBuilder()
                    .AsEphemeral()
                    .AddEmbed(embed));
        }
    }

}