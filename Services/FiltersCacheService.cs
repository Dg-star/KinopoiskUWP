using KinopoiskUWP.Models;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace KinopoiskUWP.Services
{
    public class FiltersCacheService : IFiltersCacheService
    {
        private const string CacheFileName = "filters.json";
        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        public async Task<FiltersCache> LoadAsync()
        {
            try
            {
                var file = await _localFolder.TryGetItemAsync(CacheFileName) as StorageFile;
                if (file == null) return null;

                var json = await FileIO.ReadTextAsync(file);
                return string.IsNullOrWhiteSpace(json)
                    ? null
                    : JsonSerializer.Deserialize<FiltersCache>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading filters cache: {ex.Message}");
                return null;
            }
        }

        public async Task SaveAsync(FiltersCache filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(filters, options);
                var file = await _localFolder.CreateFileAsync(
                    CacheFileName,
                    CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteTextAsync(file, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving filters cache: {ex.Message}");
                throw new InvalidOperationException("Failed to save filters cache", ex);
            }
        }
    }
}