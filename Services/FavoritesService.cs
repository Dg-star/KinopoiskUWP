using KinopoiskUWP.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;

namespace KinopoiskUWP.Services
{
    public class FavoritesService : ObservableObject, IFavoritesService
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

        public async Task LoadFavoritesAsync()
        {
            try
            {
                var file = await _localFolder.TryGetItemAsync(FavoritesFileName) as StorageFile;
                _favorites = file != null
                    ? JsonSerializer.Deserialize<List<Film>>(await FileIO.ReadTextAsync(file)) ?? new List<Film>()
                    : new List<Film>();

                await UpdateUI(() =>
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

        public async Task SaveFavoritesAsync()
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

                await FileIO.WriteTextAsync(file, JsonSerializer.Serialize(_favorites, options));
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
            if (film?.FilmId <= 0) return;

            await ExecuteOnUIThread(async () =>
            {
                if (_favorites.All(f => f.FilmId != film.FilmId))
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

            await ExecuteOnUIThread(async () =>
            {
                _favorites.RemoveAll(f => f.FilmId == film.FilmId);
                var filmToRemove = FavoriteFilms.FirstOrDefault(f => f.FilmId == film.FilmId);
                if (filmToRemove != null)
                {
                    FavoriteFilms.Remove(filmToRemove);
                }
                await SaveFavoritesAsync();
            });
        }

        public bool IsFavorite(Film film) => film != null && _favorites.Any(f => f.FilmId == film.FilmId);

        public async Task ClearFavoritesAsync()
        {
            await ExecuteOnUIThread(async () =>
            {
                _favorites.Clear();
                FavoriteFilms.Clear();
                await SaveFavoritesAsync();
            });
        }

        private async Task ExecuteOnUIThread(Action action)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () => action?.Invoke());
        }

        private async Task UpdateUI(Action action)
        {
            await ExecuteOnUIThread(action);
        }
    }
}