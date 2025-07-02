using KinopoiskUWP.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace KinopoiskUWP.Services
{
    public interface IFavoritesService
    {
        ObservableCollection<Film> FavoriteFilms { get; }
        Task LoadFavoritesAsync();
        Task SaveFavoritesAsync();
        IReadOnlyList<Film> GetFavorites();
        Task AddToFavoritesAsync(Film film);
        Task RemoveFromFavoritesAsync(Film film);
        bool IsFavorite(Film film);
        Task ClearFavoritesAsync();
    }
}