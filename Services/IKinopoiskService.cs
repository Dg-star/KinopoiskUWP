using KinopoiskUWP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KinopoiskUWP.Services
{
    public interface IKinopoiskService
    {
        Task<List<Film>> GetTopFilmsAsync();
        Task<FiltersCache> GetFiltersAsync();
        Task<Film> GetFilmDetailsAsync(int filmId);
    }
}