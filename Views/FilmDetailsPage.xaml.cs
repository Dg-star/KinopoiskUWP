using KinopoiskUWP.Models;
using KinopoiskUWP.Services;
using KinopoiskUWP.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace KinopoiskUWP.Views
{
    public sealed partial class FilmDetailsPage : Page
    {
        private readonly IKinopoiskService _kinopoiskService;
        private readonly IFavoritesService _favoritesService;

        public FilmDetailsPage()
        {
            this.InitializeComponent();

            // Получаем сервисы через DI
            _kinopoiskService = Ioc.Default.GetRequiredService<IKinopoiskService>();
            _favoritesService = Ioc.Default.GetRequiredService<IFavoritesService>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Film basicFilm)
            {
                await LoadFilmDetailsAsync(basicFilm);
            }
            else
            {
                // Обработка ошибки
                Frame.GoBack();
            }
        }

        private async Task LoadFilmDetailsAsync(Film basicFilm)
        {
            try
            {
                var detailedFilm = await _kinopoiskService.GetFilmDetailsAsync(basicFilm.FilmId);
                DataContext = new FilmDetailsViewModel(_favoritesService, detailedFilm);
            }
            catch (Exception ex)
            {
                // В UWP используем ContentDialog вместо MessageBox
                var dialog = new ContentDialog
                {
                    Title = "Ошибка",
                    Content = "Не удалось загрузить детали фильма: " + ex.Message,
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();

                DataContext = new FilmDetailsViewModel(_favoritesService, basicFilm);
            }
        }
    }
}