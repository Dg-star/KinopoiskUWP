using KinopoiskUWP.Models;
using System.Threading.Tasks;

namespace KinopoiskUWP.Services
{
    public interface IFiltersCacheService
    {
        Task<FiltersCache> LoadAsync();
        Task SaveAsync(FiltersCache filters);
    }
}