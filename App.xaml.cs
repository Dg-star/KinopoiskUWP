using KinopoiskUWP.Services;
using KinopoiskUWP.ViewModels;
using KinopoiskUWP.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KinopoiskUWP
{
    public sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Регистрация сервисов
            services.AddSingleton<IKinopoiskService, KinopoiskService>();
            services.AddSingleton<IFavoritesService, FavoritesService>();
            services.AddSingleton<IFilmsCacheService, FilmsCacheService>();
            services.AddSingleton<IFiltersCacheService, FiltersCacheService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Регистрация ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<FilmDetailsViewModel>();


            // Построение провайдера сервисов
            var serviceProvider = services.BuildServiceProvider();

            // Установка провайдера для Ioc.Default
            Ioc.Default.ConfigureServices(serviceProvider);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;

                // Инициализация NavigationService
                var navService = Ioc.Default.GetRequiredService<INavigationService>();
                navService.Initialize(rootFrame);
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage));
            }

            Window.Current.Activate();
        }
    }
}