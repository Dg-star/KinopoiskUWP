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
    public class FilmsCacheService
    {
        private const string CacheFileName = "films_cache.json";
        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        public async Task<List<Film>> LoadAsync()
        {
            try
            {
                var file = await _localFolder.TryGetItemAsync(CacheFileName) as StorageFile;
                if (file != null)
                {
                    var json = await FileIO.ReadTextAsync(file);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        return JsonSerializer.Deserialize<List<Film>>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading films cache: {ex.Message}");
            }
            return null;
        }

        public async Task SaveAsync(List<Film> films)
        {
            try
            {
                var file = await _localFolder.CreateFileAsync(
                    CacheFileName,
                    CreationCollisionOption.ReplaceExisting);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(films, options);
                await FileIO.WriteTextAsync(file, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving films cache: {ex.Message}");
                throw;
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
            }
        }
    }
}