using System.Globalization;
using DisCatSharp.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SQR.Translation;

namespace SQR;

public partial class Config
{
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("guild")]
    public ulong Guild { get; set; }
    
    [JsonProperty("fallback_language")]
    public Translator.LanguageCode FallbackLanguage { get; set; }
    
    [JsonProperty("developer_id")]
    public ulong DeveloperId { get; set; }

    [JsonIgnore]
    public DiscordUser DeveloperUser { get; set; }

    [JsonProperty("docker")] 
    public Docker Docker { get; set; }

    [JsonProperty("postgres")]
    public Postgres Postgres { get; set; }

    [JsonProperty("lavalink")]
    public Lavalink Lavalink { get; set; }
    
    [JsonProperty("activities")]
    public List<Activity> Activities { get; set; }
}

public partial class Docker
{
    [JsonProperty("enabled")] public bool Enabled { get; set; }
    [JsonProperty("startup_delay")] public int StartupDelay { get; set; }

    [JsonProperty("lavalink_host")] public string LavalinkHost { get; set; }
    
    [JsonProperty("postgres_host")] public string PostgresHost { get; set; }
}

public partial class Activity
{
    [JsonProperty("type")]
    public ActivityType Type { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("streamurl")] 
    public string StreamUrl { get; set; }
}

public partial class Postgres
{
    [JsonProperty("host")]
    public string Host { get; set; }

    [JsonProperty("port")]
    public int Port { get; set; }
    
    [JsonProperty("database")]
    public string Database { get; set; }
    
    [JsonProperty("username")]
    public string Username { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}

public partial class Lavalink
{
    [JsonProperty("host")]
    public string Host { get; set; }

    [JsonProperty("port")]
    public int Port { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}

public partial class Config
{
    public static Config FromJson(string json) => JsonConvert.DeserializeObject<Config>(json, Converter.Settings);
}

public static class Serialize
{
    public static string ToJson(this Config self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
        {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal },
            new KeysJsonConverter(typeof(Config))
        },
    };
}

public class KeysJsonConverter : JsonConverter
{
    private readonly Type[] _types;

    public KeysJsonConverter(params Type[] types)
    {
        _types = types;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JToken t = JToken.FromObject(value);

        if (t.Type != JTokenType.Object)
        {
            t.WriteTo(writer);
        }
        else
        {
            JObject o = (JObject)t;
            IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

            o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

            o.WriteTo(writer);
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotSupportedException("Unnecessary because CanRead is false. The type will skip the converter.");
    }

    public override bool CanRead => false;

    public override bool CanConvert(Type objectType)
    {
        return _types.Any(t => t == objectType);
    }
}