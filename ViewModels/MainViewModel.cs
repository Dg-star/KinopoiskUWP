using KinopoiskUWP.Models;
using KinopoiskUWP.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace KinopoiskUWP.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IKinopoiskService _kinopoiskService;
        private readonly IFavoritesService _favoritesService;
        private readonly IFiltersCacheService _filtersCacheService;
        private readonly IFilmsCacheService _filmsCacheService;
        private readonly INavigationService _navigationService;
        private bool _isLoadingFilms;

        // Observable properties (без атрибута [ObservableProperty])
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _navigationMessage;
        public string NavigationMessage
        {
            get => _navigationMessage;
            set => SetProperty(ref _navigationMessage, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private Genre _selectedGenre;
        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                if (SetProperty(ref _selectedGenre, value))
                {
                    _ = LoadFilmsAsync();
                }
            }
        }

        private Country _selectedCountry;
        public Country SelectedCountry
        {
            get => _selectedCountry;
            set
            {
                if (SetProperty(ref _selectedCountry, value))
                {
                    _ = LoadFilmsAsync();
                }
            }
        }

        public bool IsNotLoading => !IsLoading;

        public ObservableCollection<Film> Films { get; } = new ObservableCollection<Film>();
        public ObservableCollection<Genre> Genres { get; } = new ObservableCollection<Genre>();
        public ObservableCollection<Country> Countries { get; } = new ObservableCollection<Country>();

        public IAsyncRelayCommand LoadFilmsCommand { get; }
        public IAsyncRelayCommand<Film> NavigateToFilmDetailsCommand { get; }
        public IAsyncRelayCommand<Film> AddToFavoritesCommand { get; }
        public IAsyncRelayCommand RefreshDataCommand { get; }

        public MainViewModel()
        {
            _kinopoiskService = Ioc.Default.GetRequiredService<IKinopoiskService>();
            _favoritesService = Ioc.Default.GetRequiredService<IFavoritesService>();
            _filtersCacheService = Ioc.Default.GetRequiredService<IFiltersCacheService>();
            _filmsCacheService = Ioc.Default.GetRequiredService<IFilmsCacheService>();
            _navigationService = Ioc.Default.GetRequiredService<INavigationService>();

            LoadFilmsCommand = new AsyncRelayCommand(LoadFilmsAsync);
            NavigateToFilmDetailsCommand = new AsyncRelayCommand<Film>(NavigateToFilmDetailsAsync);
            AddToFavoritesCommand = new AsyncRelayCommand<Film>(AddToFavoritesAsync);
            RefreshDataCommand = new AsyncRelayCommand(RefreshDataAsync);

            _ = InitializeAsync();
        }

        public void OnNavigatedTo(object parameter)
        {
            NavigationMessage = string.Empty;

            if (parameter == null)
            {
                NavigationMessage = "Добро пожаловать!";
                return;
            }

            if (parameter is string message)
            {
                NavigationMessage = message;
                return;
            }

            if (parameter is int filmId)
            {
                NavigationMessage = $"Вы перешли с фильма ID: {filmId}";
                return;
            }

            if (parameter is Film film)
            {
                NavigationMessage = $"Вы смотрите: {film.NameRu}";
                return;
            }

            if (parameter is Dictionary<string, object> filters)
            {
                ProcessNavigationFilters(filters);
                return;
            }

            NavigationMessage = "Получен неизвестный параметр навигации";
        }

        private void ProcessNavigationFilters(Dictionary<string, object> filters)
        {
            var messages = new List<string>();

            if (filters.TryGetValue("genre", out var genre) && genre is string genreName)
            {
                SelectedGenre = Genres.FirstOrDefault(g => g.Name == genreName) ?? Genres.First();
                messages.Add($"Жанр: {genreName}");
            }

            if (filters.TryGetValue("country", out var country) && country is string countryName)
            {
                SelectedCountry = Countries.FirstOrDefault(c => c.Name == countryName) ?? Countries.First();
                messages.Add($"Страна: {countryName}");
            }

            if (filters.TryGetValue("year", out var year))
            {
                messages.Add($"Год: {year}");
            }

            NavigationMessage = string.Join(" | ", messages);
        }

        // Остальные методы остаются без изменений
        public async Task NavigateToFilmDetailsAsync(Film film)
        {
            if (film == null) return;
            await _navigationService.NavigateToFilmDetailsAsync(film);
        }

        public async Task AddToFavoritesAsync(Film film)
        {
            if (film == null) return;

            try
            {
                await _favoritesService.AddToFavoritesAsync(film);
                film.IsFavorite = true;
                await UpdateFilmInCollection(film);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding to favorites: {ex.Message}");
                ErrorMessage = "Не удалось добавить в избранное";
            }
        }

        public async Task LoadFilmsAsync()
        {
            if (_isLoadingFilms) return;
            _isLoadingFilms = true;

            try
            {
                await UpdateUI(() =>
                {
                    IsLoading = true;
                    ErrorMessage = null;
                    Films.Clear();
                });

                var films = await GetFilmsWithCacheAsync();
                films = ApplyFilters(films);

                await UpdateFilmsCollectionAsync(films);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading films: {ex}");
                ErrorMessage = "Не удалось загрузить фильмы";
            }
            finally
            {
                _isLoadingFilms = false;
                IsLoading = false;
            }
        }

        public async Task RefreshDataAsync()
        {
            await _filmsCacheService.ClearCacheAsync();
            await InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadFiltersAsync();
            await LoadFilmsAsync();
        }

        private async Task<List<Film>> GetFilmsWithCacheAsync()
        {
            var cachedFilms = await _filmsCacheService.LoadAsync();
            if (cachedFilms?.Count > 0) return cachedFilms;

            var films = await _kinopoiskService.GetTopFilmsAsync();
            films = films.GroupBy(f => f.FilmId).Select(g => g.First()).ToList();
            await _filmsCacheService.SaveAsync(films);
            return films;
        }

        private List<Film> ApplyFilters(List<Film> films)
        {
            if (SelectedGenre?.Id != 0)
            {
                films = films.Where(f =>
                    f.Genres?.Any(g =>
                        string.Equals(g.Name, SelectedGenre?.Name, StringComparison.OrdinalIgnoreCase))
                    ?? false).ToList();
            }

            if (SelectedCountry?.Id != 0)
            {
                films = films.Where(f =>
                    f.Countries?.Any(c =>
                        string.Equals(c.Name, SelectedCountry?.Name, StringComparison.OrdinalIgnoreCase))
                    ?? false).ToList();
            }

            return films;
        }

        private async Task UpdateFilmsCollectionAsync(List<Film> films)
        {
            await UpdateUI(() =>
            {
                foreach (var film in films)
                {
                    film.IsFavorite = _favoritesService.IsFavorite(film);
                    Films.Add(film);
                }

                if (Films.Count == 0)
                {
                    ErrorMessage = "Не найдено фильмов по выбранным фильтрам";
                }
            });
        }

        private async Task UpdateFilmInCollection(Film film)
        {
            await UpdateUI(() =>
            {
                var existingFilm = Films.FirstOrDefault(f => f.FilmId == film.FilmId);
                if (existingFilm != null)
                {
                    existingFilm.IsFavorite = film.IsFavorite;
                }
            });
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                var filters = await _filtersCacheService.LoadAsync() ??
                             await LoadAndCacheFiltersAsync();

                await UpdateFilterCollectionsAsync(filters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading filters: {ex}");
                ErrorMessage = "Не удалось загрузить фильтры";
            }
        }

        private async Task<FiltersCache> LoadAndCacheFiltersAsync()
        {
            var filters = await _kinopoiskService.GetFiltersAsync();
            await _filtersCacheService.SaveAsync(filters);
            return filters;
        }

        private async Task UpdateFilterCollectionsAsync(FiltersCache filters)
        {
            await UpdateUI(() =>
            {
                UpdateGenresCollection(filters.Genres);
                UpdateCountriesCollection(filters.Countries);

                SelectedGenre = Genres.FirstOrDefault();
                SelectedCountry = Countries.FirstOrDefault();
            });
        }

        private void UpdateGenresCollection(IEnumerable<Genre> genres)
        {
            Genres.Clear();
            Genres.Add(new Genre { Id = 0, Name = "Все жанры" });
            foreach (var genre in genres
                .Where(g => !string.IsNullOrWhiteSpace(g.Name))
                .OrderBy(g => g.Name))
            {
                Genres.Add(genre);
            }
        }

        private void UpdateCountriesCollection(IEnumerable<Country> countries)
        {
            Countries.Clear();
            Countries.Add(new Country { Id = 0, Name = "Все страны" });
            foreach (var country in countries
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .OrderBy(c => c.Name))
            {
                Countries.Add(country);
            }
        }

        private async Task UpdateUI(Action action)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                action?.Invoke();
            });
        }
    }
}