using Newtonsoft.Json;

#pragma warning disable CS8618

namespace SQR.Translation.Language.Music;

public class ShuffleCommand
{
    [JsonProperty("shuffled")]
    public string Shuffled { get; set; }
}