using KinopoiskUWP.Services;
using KinopoiskUWP.ViewModels;
using KinopoiskUWP.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Diagnostics;
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
            this.UnhandledException += OnUnhandledException;
            ConfigureServices();
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Debug.WriteLine($"CRITICAL ERROR: {e.Exception}");
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
            var rootFrame = Window.Current.Content as Frame ?? new Frame();

            // Получаем сервис навигации один раз
            var navService = Ioc.Default.GetRequiredService<INavigationService>();

            if (Window.Current.Content == null) // Проверяем, а не rootFrame
            {
                // Инициализация NavigationService
                navService.Initialize(rootFrame);
                Window.Current.Content = rootFrame;

                try
                {
                    if (rootFrame.Content == null)
                    {
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Initial navigation failed: {ex}");
                    // Здесь можно добавить обработку ошибки навигации
                }
            }

            Window.Current.Activate();
        }
    }
}