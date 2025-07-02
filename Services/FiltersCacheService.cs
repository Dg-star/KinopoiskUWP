using KinopoiskUWP.Models;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace KinopoiskUWP.Services
{
    public class FiltersCacheService
    {
        private const string CacheFileName = "filters.json";
        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        public async Task<FiltersCache> LoadAsync()
        {
            try
            {
                var file = await _localFolder.TryGetItemAsync(CacheFileName) as StorageFile;
                if (file != null)
                {
                    var json = await FileIO.ReadTextAsync(file);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        return JsonSerializer.Deserialize<FiltersCache>(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading filters cache: {ex.Message}");
            }
            return null;
        }

        public async Task SaveAsync(FiltersCache filters)
        {
            try
            {
                var file = await _localFolder.CreateFileAsync(
                    CacheFileName,
                    CreationCollisionOption.ReplaceExisting);

                var json = JsonSerializer.Serialize(
                    filters,
                    new JsonSerializerOptions { WriteIndented = true });

                await FileIO.WriteTextAsync(file, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving filters cache: {ex.Message}");
                throw;
            }
        }
    }
}