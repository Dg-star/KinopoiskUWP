using KinopoiskUWP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KinopoiskUWP.Services
{
    public class KinopoiskService : IKinopoiskService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "e7534db3-388a-487b-bc0a-14ed9e1d4be5";
        private const string BaseUrl = "https://kinopoiskapiunofficial.tech/api/v2.2/films";

        public KinopoiskService()
        {
            _httpClient = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            });

            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<Film>> GetTopFilmsAsync()
        {
            var url = $"{BaseUrl}/top?type=TOP_100_POPULAR_FILMS&page=1";
            var result = await ExecuteApiRequestAsync<KinopoiskApiResponse>(url);
            return result?.Films ?? new List<Film>();
        }

        public async Task<FiltersCache> GetFiltersAsync()
        {
            var url = $"{BaseUrl}/filters";
            return await ExecuteApiRequestAsync<FiltersCache>(url);
        }

        public async Task<Film> GetFilmDetailsAsync(int filmId)
        {
            var url = $"{BaseUrl}/{filmId}";
            return await ExecuteApiRequestAsync<Film>(url);
        }

        private async Task<T> ExecuteApiRequestAsync<T>(string url) where T : class
        {
            try
            {
                Debug.WriteLine($"API Request: {url}");

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Response: {jsonString}");

                if (IsHtmlResponse(jsonString))
                {
                    throw new KinopoiskApiException("Сервер вернул HTML вместо JSON. Возможно проблема с API ключом.");
                }

                return JsonSerializer.Deserialize<T>(jsonString, SourceGenerationContext.Default.Options);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new KinopoiskApiException("Неверный API ключ. Проверьте правильность ключа.");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KinopoiskApiException("Запрошенный ресурс не найден.");
            }
            catch (HttpRequestException ex) when ((int?)ex.StatusCode == 429)
            {
                throw new KinopoiskApiException("Превышен лимит запросов. Попробуйте позже.");
            }
            catch (HttpRequestException ex)
            {
                throw new KinopoiskApiException($"Ошибка HTTP: {ex.StatusCode}", ex);
            }
            catch (JsonException ex)
            {
                throw new KinopoiskApiException("Ошибка обработки данных от сервера", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new KinopoiskApiException("Превышено время ожидания ответа от сервера", ex);
            }
            catch (Exception ex)
            {
                throw new KinopoiskApiException("Произошла непредвиденная ошибка", ex);
            }
        }

        private bool IsHtmlResponse(string content)
        {
            return !string.IsNullOrEmpty(content) &&
                  (content.TrimStart().StartsWith("<!DOCTYPE html>") ||
                   content.Contains("<html>"));
        }
    }

    public class KinopoiskApiResponse
    {
        public int PagesCount { get; set; }
        public List<Film> Films { get; set; }
    }
}