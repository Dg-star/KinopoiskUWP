using KinopoiskUWP.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KinopoiskUWP.Services
{
    [JsonSourceGenerationOptions(
        GenerationMode = JsonSourceGenerationMode.Metadata,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString)]
    [JsonSerializable(typeof(Film))]
    [JsonSerializable(typeof(List<Film>))]
    [JsonSerializable(typeof(KinopoiskApiResponse))]
    [JsonSerializable(typeof(FiltersCache))]
    [JsonSerializable(typeof(Genre))]
    [JsonSerializable(typeof(List<Genre>))]
    [JsonSerializable(typeof(Country))]
    [JsonSerializable(typeof(List<Country>))]
    public partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}