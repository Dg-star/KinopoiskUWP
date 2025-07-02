using KinopoiskUWP.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace KinopoiskUWP.Services
{
    public class FavoritesService : ObservableObject
    {
        private const string FavoritesFileName = "favorites.json";
        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
        private List<Film> _favorites = new List<Film>();

        public ObservableCollection<Film> FavoriteFilms { get; } = new ObservableCollection<Film>();

        public FavoritesService()
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadFavoritesAsync();
        }

        private async Task LoadFavoritesAsync()
        {
            try
            {
                var file = await _localFolder.TryGetItemAsync(FavoritesFileName) as StorageFile;
                if (file != null)
                {
                    var json = await FileIO.ReadTextAsync(file);
                    _favorites = JsonSerializer.Deserialize<List<Film>>(json) ?? new List<Film>();
                }
                else
                {
                    _favorites = new List<Film>();
                }

                await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        FavoriteFilms.Clear();
                        foreach (var film in _favorites)
                        {
                            FavoriteFilms.Add(film);
                        }
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading favorites: {ex.Message}");
                _favorites = new List<Film>();
            }
        }

        private async Task SaveFavoritesAsync()
        {
            try
            {
                var file = await _localFolder.CreateFileAsync(
                    FavoritesFileName,
                    CreationCollisionOption.ReplaceExisting);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(_favorites, options);
                await FileIO.WriteTextAsync(file, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving favorites: {ex.Message}");
                throw;
            }
        }

        public IReadOnlyList<Film> GetFavorites() => _favorites.AsReadOnly();

        public async Task AddToFavoritesAsync(Film film)
        {
            if (film == null || film.FilmId <= 0) return;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    if (!_favorites.Exists(f => f.FilmId == film.FilmId))
                    {
                        _favorites.Add(film);
                        FavoriteFilms.Add(film);
                        await SaveFavoritesAsync();
                    }
                });
        }

        public async Task RemoveFromFavoritesAsync(Film film)
        {
            if (film == null) return;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    _favorites.RemoveAll(f => f.FilmId == film.FilmId);
                    var filmInCollection = FavoriteFilms.FirstOrDefault(f => f.FilmId == film.FilmId);
                    if (filmInCollection != null)
                    {
                        FavoriteFilms.Remove(filmInCollection);
                    }
                    await SaveFavoritesAsync();
                });
        }

        public bool IsFavorite(Film film)
        {
            return film != null && _favorites.Exists(f => f.FilmId == film.FilmId);
        }

        public async Task ClearFavoritesAsync()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    _favorites.Clear();
                    FavoriteFilms.Clear();
                    await SaveFavoritesAsync();
                });
        }
    }
}