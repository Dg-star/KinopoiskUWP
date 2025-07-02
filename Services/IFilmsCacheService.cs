using KinopoiskUWP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KinopoiskUWP.Services
{
    public interface IFilmsCacheService
    {
        Task<List<Film>> LoadAsync();
        Task SaveAsync(List<Film> films);
        Task ClearCacheAsync();
    }
}