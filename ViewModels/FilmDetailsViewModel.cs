using KinopoiskUWP.Models;
using KinopoiskUWP.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace KinopoiskUWP.ViewModels
{
    public class FilmDetailsViewModel : ObservableObject
    {
        private readonly IFavoritesService _favoritesService;
        private Film _film;

        public FilmDetailsViewModel(IFavoritesService favoritesService, Film film)
        {
            _favoritesService = favoritesService ?? throw new ArgumentNullException(nameof(favoritesService));
            Film = film ?? throw new ArgumentNullException(nameof(film));

            AddToFavoritesCommand = new AsyncRelayCommand(AddToFavoritesAsync);
            OpenKinopoiskCommand = new AsyncRelayCommand(OpenKinopoiskAsync);
        }

        public Film Film
        {
            get => _film;
            private set => SetProperty(ref _film, value);
        }

        public string NameRu => Film.NameRu;
        public string PosterUrlPreview => Film.PosterUrlPreview;
        public string Year => Film.Year;
        public string GenresString => Film.Genres != null ? string.Join(", ", Film.Genres.Select(g => g.Name)) : string.Empty;
        public string CountriesString => Film.Countries != null ? string.Join(", ", Film.Countries.Select(c => c.Name)) : string.Empty;
        public string NameOriginal => Film.NameOriginal ?? string.Empty;
        public string NameEn => Film.NameEn ?? string.Empty;
        public string ShortDescription => Film.ShortDescription ?? string.Empty;
        public double? RatingKinopoisk => Film.RatingKinopoisk;
        public double? RatingImdb => Film.RatingImdb;
        public double? RatingFilmCritics => Film.RatingFilmCritics;
        public string Slogan => Film.Slogan ?? string.Empty;
        public string Description => Film.Description ?? string.Empty;
        public string ImdbId => Film.ImdbId ?? string.Empty;
        public string WebUrl => Film.WebUrl ?? string.Empty;
        public bool IsFavorite => Film.IsFavorite;

        public IAsyncRelayCommand AddToFavoritesCommand { get; }
        public IAsyncRelayCommand OpenKinopoiskCommand { get; }

        private async Task AddToFavoritesAsync()
        {
            if (Film == null) return;

            try
            {
                if (IsFavorite)
                {
                    await _favoritesService.RemoveFromFavoritesAsync(Film);
                }
                else
                {
                    await _favoritesService.AddToFavoritesAsync(Film);
                }

                Film.IsFavorite = !IsFavorite;
                OnPropertyChanged(nameof(IsFavorite));
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync($"Ошибка добавления в избранное: {ex.Message}");
            }
        }

        private async Task OpenKinopoiskAsync()
        {
            if (!string.IsNullOrEmpty(WebUrl))
            {
                try
                {
                    await Launcher.LaunchUriAsync(new Uri(WebUrl));
                }
                catch (Exception ex)
                {
                    await ShowErrorDialogAsync($"Ошибка открытия ссылки: {ex.Message}");
                }
            }
        }

        private async Task ShowErrorDialogAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Ошибка",
                Content = message,
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }
    }
}