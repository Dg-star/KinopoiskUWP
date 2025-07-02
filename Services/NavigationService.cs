using KinopoiskUWP.Models;
using KinopoiskUWP.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KinopoiskUWP.Services
{
    public class NavigationService : INavigationService
    {
        private Frame _mainFrame;

        public void Initialize(Frame frame)
        {
            _mainFrame = frame;
        }

        public void Navigate<T>() where T : Page
        {
            _mainFrame?.Navigate(typeof(T));
        }

        public void Navigate(Type pageType)
        {
            _mainFrame?.Navigate(pageType);
        }

        public void Navigate<T>(object parameter) where T : Page
        {
            _mainFrame?.Navigate(typeof(T), parameter);
        }

        public void Navigate(Type pageType, object parameter)
        {
            _mainFrame?.Navigate(pageType, parameter);
        }

        public async Task NavigateToFilmDetailsAsync(Film film)
        {
            if (film == null) return;

            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _mainFrame?.Navigate(typeof(FilmDetailsPage), film.FilmId);
            });
        }

        public void GoBack()
        {
            if (_mainFrame?.CanGoBack == true)
            {
                _mainFrame.GoBack();
            }
        }
    }
}