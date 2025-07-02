using KinopoiskUWP.Models;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace KinopoiskUWP.Services
{
    public interface INavigationService
    {
        void Initialize(Frame frame);
        void Navigate<T>() where T : Page;
        void Navigate(Type pageType);
        void Navigate<T>(object parameter) where T : Page;
        void Navigate(Type pageType, object parameter);
        Task NavigateToFilmDetailsAsync(Film film);
        void GoBack();
    }
}