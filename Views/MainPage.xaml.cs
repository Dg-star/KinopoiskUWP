using KinopoiskUWP.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

                    // Обновление интерфейса через Dispatcher
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        // Правильный способ обновления - через метод в ViewModel
                        ViewModel.NotifyFilmsUpdated();
                    });
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine($"Page load error: {ex}");
                    ViewModel.ErrorMessage = "Ошибка при загрузке страницы";
                }
            };
        }
    }
}