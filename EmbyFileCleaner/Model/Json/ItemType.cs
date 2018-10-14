using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EmbyFileCleaner.Model.Json
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemType
    {
        Episode,
        Movie
    }
}
