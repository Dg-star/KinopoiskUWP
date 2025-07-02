using KinopoiskUWP.Models;
using KinopoiskUWP.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace KinopoiskUWP.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = Ioc.Default.GetRequiredService<MainViewModel>();
            DataContext = ViewModel;

            Debug.WriteLine("MainPage initialized");

            this.Loaded += async (s, e) =>
            {
                Debug.WriteLine("MainPage loaded");
                try
                {
                    await ViewModel.LoadFilmsAsync();
                    Debug.WriteLine($"Films loaded: {ViewModel.Films.Count}");
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"Page load error: {ex}");
                    ViewModel.ErrorMessage = "Ошибка при загрузке страницы";
                }
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Debug.WriteLine($"MainPage navigated to with parameter: {e?.Parameter}");
            ViewModel.OnNavigatedTo(e?.Parameter);
        }

        private async void FilmItem_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Film film)
            {
                Debug.WriteLine($"Film clicked: {film.NameRu}");
                await ViewModel.NavigateToFilmDetailsAsync(film);
            }
        }
    }
}