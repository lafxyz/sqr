using Newtonsoft.Json;
using SQR.Translation.Music;

#pragma warning disable CS8618

namespace SQR.Translation;

public class DevTranslation
{
    [JsonProperty("general")]
    public DevGeneralTranslation GeneralTranslation { get; set; }

    [JsonProperty("reload_command")]
    public ReloadCommandTranslation ReloadCommandTranslation { get; set; }
}