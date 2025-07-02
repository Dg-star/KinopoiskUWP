using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KinopoiskUWP.Models
{
    public class Film : ObservableObject
    {
        [JsonPropertyName("filmId")]
        public int FilmId { get; init; }

        [JsonPropertyName("nameRu")]
        public string NameRu { get; init; }

        [JsonPropertyName("nameEn")]
        public string NameEn { get; init; }

        [JsonPropertyName("nameOriginal")]
        public string NameOriginal { get; init; }

        [JsonPropertyName("posterUrl")]
        public string PosterUrl { get; init; }

        [JsonPropertyName("posterUrlPreview")]
        public string PosterUrlPreview { get; init; }

        [JsonPropertyName("coverUrl")]
        public string CoverUrl { get; init; }

        [JsonPropertyName("logoUrl")]
        public string LogoUrl { get; init; }

        [JsonPropertyName("year")]
        public string Year { get; init; }

        [JsonPropertyName("filmLength")]
        [JsonConverter(typeof(FilmLengthConverter))]
        public string FilmLength { get; init; }

        [JsonPropertyName("slogan")]
        public string Slogan { get; init; }

        [JsonPropertyName("description")]
        public string Description { get; init; }

        [JsonPropertyName("shortDescription")]
        public string ShortDescription { get; init; }

        [JsonPropertyName("ratingKinopoisk")]
        public double? RatingKinopoisk { get; init; }

        [JsonPropertyName("ratingKinopoiskVoteCount")]
        public int? RatingKinopoiskVoteCount { get; init; }

        [JsonPropertyName("ratingImdb")]
        public double? RatingImdb { get; init; }

        [JsonPropertyName("ratingImdbVoteCount")]
        public int? RatingImdbVoteCount { get; init; }

        [JsonPropertyName("ratingFilmCritics")]
        public double? RatingFilmCritics { get; init; }

        [JsonPropertyName("ratingFilmCriticsVoteCount")]
        public int? RatingFilmCriticsVoteCount { get; init; }

        [JsonPropertyName("ratingAwait")]
        public double? RatingAwait { get; init; }

        [JsonPropertyName("ratingAwaitCount")]
        public int? RatingAwaitCount { get; init; }

        [JsonPropertyName("ratingRfCritics")]
        public double? RatingRfCritics { get; init; }

        [JsonPropertyName("ratingRfCriticsVoteCount")]
        public int? RatingRfCriticsVoteCount { get; init; }

        [JsonPropertyName("webUrl")]
        public string WebUrl { get; init; }

        [JsonPropertyName("kinopoiskId")]
        public int? KinopoiskId { get; init; }

        [JsonPropertyName("kinopoiskHDId")]
        public string KinopoiskHDId { get; init; }

        [JsonPropertyName("imdbId")]
        public string ImdbId { get; init; }

        [JsonPropertyName("reviewsCount")]
        public int? ReviewsCount { get; init; }

        [JsonPropertyName("ratingGoodReview")]
        public double? RatingGoodReview { get; init; }

        [JsonPropertyName("ratingGoodReviewVoteCount")]
        public int? RatingGoodReviewVoteCount { get; init; }

        [JsonPropertyName("editorAnnotation")]
        public string EditorAnnotation { get; init; }

        [JsonPropertyName("isTicketsAvailable")]
        public bool? IsTicketsAvailable { get; init; }

        [JsonPropertyName("productionStatus")]
        public string ProductionStatus { get; init; }

        [JsonPropertyName("type")]
        public string Type { get; init; }

        [JsonPropertyName("ratingMpaa")]
        public string RatingMpaa { get; init; }

        [JsonPropertyName("ratingAgeLimits")]
        public string RatingAgeLimits { get; init; }

        [JsonPropertyName("hasImax")]
        public bool? HasImax { get; init; }

        [JsonPropertyName("has3D")]
        public bool? Has3D { get; init; }

        [JsonPropertyName("lastSync")]
        public DateTime? LastSync { get; init; }

        [JsonPropertyName("startYear")]
        public int? StartYear { get; init; }

        [JsonPropertyName("endYear")]
        public int? EndYear { get; init; }

        [JsonPropertyName("serial")]
        public bool? Serial { get; init; }

        [JsonPropertyName("shortFilm")]
        public bool? ShortFilm { get; init; }

        [JsonPropertyName("completed")]
        public bool? Completed { get; init; }

        [JsonPropertyName("countries")]
        public List<Country> Countries { get; init; } = new List<Country>();

        [JsonPropertyName("genres")]
        public List<Genre> Genres { get; init; } = new List<Genre>();

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }
    }

    public class FilmLengthConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32().ToString();
            }
            return reader.GetString() ?? string.Empty;
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    public class FiltersCache
    {
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public List<Country> Countries { get; set; } = new List<Country>();
    }

    public class Genre
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("genre")]
        public string Name { get; set; }

        [JsonIgnore]
        public int? GenreId
        {
            set { if (value.HasValue) Id = value; }
        }
    }

    public class Country
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("country")]
        public string Name { get; set; }

        [JsonIgnore]
        public int? CountryId
        {
            set { if (value.HasValue) Id = value; }
        }
    }
}