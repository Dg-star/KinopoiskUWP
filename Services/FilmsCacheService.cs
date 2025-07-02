using KinopoiskUWP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace KinopoiskUWP.Services
{
    public class FilmsCacheService : IFilmsCacheService
    {
        private const string CacheFileName = "films_cache.json";
        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        public async Task<List<Film>> LoadAsync()
        {
            try
            {
                var file = await _localFolder.TryGetItemAsync(CacheFileName) as StorageFile;
                if (file == null) return null;

                var json = await FileIO.ReadTextAsync(file);
                return string.IsNullOrWhiteSpace(json)
                    ? null
                    : JsonSerializer.Deserialize<List<Film>>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading films cache: {ex.Message}");
                return null;
            }
        }

        public async Task SaveAsync(List<Film> films)
        {
            if (films == null) throw new ArgumentNullException(nameof(films));

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(films, options);
                var file = await _localFolder.CreateFileAsync(
                    CacheFileName,
                    CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteTextAsync(file, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving films cache: {ex.Message}");
                throw new InvalidOperationException("Failed to save films cache", ex);
            }
        }

        public async Task ClearCacheAsync()
        {
            try
            {
                var file = await _localFolder.TryGetItemAsync(CacheFileName) as StorageFile;
                if (file != null)
                {
                    await file.DeleteAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing films cache: {ex.Message}");
                throw new InvalidOperationException("Failed to clear films cache", ex);
            }
        }
    }
}