using Newtonsoft.Json;
using SQR.Translation.Music;

#pragma warning disable CS8618

namespace SQR.Translation;

public class Dev
{
    [JsonProperty("general")]
    public DevGeneral General { get; set; }

    [JsonProperty("reload_command")]
    public ReloadCommand ReloadCommand { get; set; }
}