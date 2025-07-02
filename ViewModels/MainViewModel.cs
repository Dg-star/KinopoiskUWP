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
    public class MainViewModel : ObservableObject
    {
        private readonly IKinopoiskService _kinopoiskService;
        private readonly IFavoritesService _favoritesService;
        private readonly IFiltersCacheService _filtersCacheService;
        private readonly IFilmsCacheService _filmsCacheService;
        private readonly INavigationService _navigationService;

        private bool _isLoading;
        private bool _isLoadingFilms;
        private string _errorMessage;
        private string _navigationMessage;
        private Genre _selectedGenre;
        private Country _selectedCountry;

        public ObservableCollection<Film> Films { get; } = new ObservableCollection<Film>();
        public ObservableCollection<Genre> Genres { get; } = new ObservableCollection<Genre>();
        public ObservableCollection<Country> Countries { get; } = new ObservableCollection<Country>();

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string NavigationMessage
        {
            get => _navigationMessage;
            private set => SetProperty(ref _navigationMessage, value);
        }

        public Genre SelectedGenre
        {
            get => _selectedGenre ?? Genres.FirstOrDefault();
            set
            {
                if (SetProperty(ref _selectedGenre, value))
                {
                    Debug.WriteLine($"Genre selected: {value?.Name}");
                    _ = LoadFilmsAsync();
                }
            }
        }

        public Country SelectedCountry
        {
            get => _selectedCountry ?? Countries.FirstOrDefault();
            set
            {
                if (SetProperty(ref _selectedCountry, value))
                {
                    Debug.WriteLine($"Country selected: {value?.Name}");
                    _ = LoadFilmsAsync();
                }
            }
        }

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

            LoadFilmsCommand = new AsyncRelayCommand(LoadFilmsAsync, () => !_isLoadingFilms);
            NavigateToFilmDetailsCommand = new AsyncRelayCommand<Film>(NavigateToFilmDetailsAsync, film => film != null);
            AddToFavoritesCommand = new AsyncRelayCommand<Film>(AddToFavoritesAsync, film => film != null && !film.IsFavorite);
            RefreshDataCommand = new AsyncRelayCommand(RefreshDataAsync, () => !_isLoadingFilms);

            Debug.WriteLine("MainViewModel created");
            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("Initializing MainViewModel...");
                await LoadFiltersAsync();
                await LoadFilmsAsync();
                Debug.WriteLine("MainViewModel initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initialize error: {ex}");
                ErrorMessage = "Ошибка инициализации";
            }
        }

        public async Task LoadFilmsAsync()
        {
            if (_isLoadingFilms) return;
            _isLoadingFilms = true;
            LoadFilmsCommand.NotifyCanExecuteChanged();
            RefreshDataCommand.NotifyCanExecuteChanged();

            try
            {
                Debug.WriteLine("Loading films...");
                await UpdateUI(() =>
                {
                    IsLoading = true;
                    ErrorMessage = null;
                    Films.Clear();
                });

                var films = await GetFilmsWithCacheAsync();
                Debug.WriteLine($"Received {films?.Count ?? 0} films");

                if (films == null || films.Count == 0)
                {
                    Debug.WriteLine("No films loaded");
                    ErrorMessage = "Фильмы не найдены";
                    return;
                }

                films = ApplyFilters(films);
                Debug.WriteLine($"After filtering: {films.Count} films");

                await UpdateFilmsCollectionAsync(films);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load films error: {ex}");
                ErrorMessage = "Ошибка загрузки фильмов";
            }
            finally
            {
                _isLoadingFilms = false;
                IsLoading = false;
                LoadFilmsCommand.NotifyCanExecuteChanged();
                RefreshDataCommand.NotifyCanExecuteChanged();
                Debug.WriteLine("Films loading completed");
            }
        }

        private async Task<List<Film>> GetFilmsWithCacheAsync()
        {
            try
            {
                Debug.WriteLine("Checking film cache...");
                var cachedFilms = await _filmsCacheService.LoadAsync();

                if (cachedFilms?.Count > 0)
                {
                    Debug.WriteLine($"Loaded {cachedFilms.Count} films from cache");
                    return cachedFilms;
                }

                Debug.WriteLine("No cache found, loading from API...");
                var films = await _kinopoiskService.GetTopFilmsAsync();
                films = films?.Where(f => f != null).GroupBy(f => f.FilmId).Select(g => g.First()).ToList()
                        ?? new List<Film>();

                Debug.WriteLine($"Received {films.Count} films from API");

                if (films.Count > 0)
                {
                    Debug.WriteLine("Saving films to cache...");
                    await _filmsCacheService.SaveAsync(films);
                }

                return films;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cache error: {ex}");
                return new List<Film>();
            }
        }

        private List<Film> ApplyFilters(List<Film> films)
        {
            if (films == null) return new List<Film>();

            Debug.WriteLine($"Applying filters to {films.Count} films");
            Debug.WriteLine($"Selected Genre: {SelectedGenre?.Name ?? "None"}");
            Debug.WriteLine($"Selected Country: {SelectedCountry?.Name ?? "None"}");

            var filteredFilms = films.ToList();

            // Для тестирования - временно отключим фильтрацию
            return filteredFilms;

            /* Рабочий код фильтрации (закомментирован для теста)
            if (SelectedGenre != null && SelectedGenre.Id != 0)
            {
                filteredFilms = filteredFilms.Where(f =>
                    f.Genres?.Any(g => g != null && g.Name == SelectedGenre.Name) ?? false)
                    .ToList();
            }

            if (SelectedCountry != null && SelectedCountry.Id != 0)
            {
                filteredFilms = filteredFilms.Where(f =>
                    f.Countries?.Any(c => c != null && c.Name == SelectedCountry.Name) ?? false)
                    .ToList();
            }

            return filteredFilms;
            */
        }

        private async Task UpdateFilmsCollectionAsync(List<Film> films)
        {
            await UpdateUI(() =>
            {
                Films.Clear();

                if (films == null || films.Count == 0)
                {
                    Debug.WriteLine("No films to display");
                    ErrorMessage = "Фильмы не найдены по выбранным фильтрам";
                    return;
                }

                foreach (var film in films)
                {
                    if (film != null)
                    {
                        Debug.WriteLine($"Adding film: {film.NameRu} (ID: {film.FilmId})");
                        film.IsFavorite = _favoritesService.IsFavorite(film);
                        Films.Add(film);
                    }
                }
            });
        }

        private async Task LoadFiltersAsync()
        {
            try
            {
                Debug.WriteLine("Loading filters...");
                var filters = await _filtersCacheService.LoadAsync() ??
                             await LoadAndCacheFiltersAsync();

                await UpdateFilterCollectionsAsync(filters);
                Debug.WriteLine("Filters loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load filters error: {ex}");
                ErrorMessage = "Ошибка загрузки фильтров";
            }
        }

        private async Task<FiltersCache> LoadAndCacheFiltersAsync()
        {
            Debug.WriteLine("Loading filters from API...");
            var filters = await _kinopoiskService.GetFiltersAsync();

            if (filters != null)
            {
                Debug.WriteLine("Saving filters to cache...");
                await _filtersCacheService.SaveAsync(filters);
            }

            return filters ?? new FiltersCache();
        }

        private async Task UpdateFilterCollectionsAsync(FiltersCache filters)
        {
            await UpdateUI(() =>
            {
                Genres.Clear();
                Genres.Add(new Genre { Id = 0, Name = "Все жанры" });

                if (filters?.Genres != null)
                {
                    foreach (var genre in filters.Genres
                        .Where(g => !string.IsNullOrEmpty(g?.Name)))
                    {
                        Genres.Add(genre);
                    }
                }

                Countries.Clear();
                Countries.Add(new Country { Id = 0, Name = "Все страны" });

                if (filters?.Countries != null)
                {
                    foreach (var country in filters.Countries
                        .Where(c => !string.IsNullOrEmpty(c?.Name)))
                    {
                        Countries.Add(country);
                    }
                }

                SelectedGenre = Genres.FirstOrDefault();
                SelectedCountry = Countries.FirstOrDefault();

                Debug.WriteLine($"Loaded {Genres.Count} genres and {Countries.Count} countries");
            });
        }

        public async Task NavigateToFilmDetailsAsync(Film film)
        {
            if (film == null) return;

            try
            {
                Debug.WriteLine($"Navigating to film details: {film.NameRu}");
                await _navigationService.NavigateToFilmDetailsAsync(film);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex}");
                ErrorMessage = "Ошибка навигации";
            }
        }

        public async Task AddToFavoritesAsync(Film film)
        {
            if (film == null || film.IsFavorite) return;

            try
            {
                Debug.WriteLine($"Adding film to favorites: {film.NameRu}");
                await _favoritesService.AddToFavoritesAsync(film);
                film.IsFavorite = true;
                await UpdateFilmInCollection(film);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Add favorite error: {ex}");
                ErrorMessage = "Ошибка добавления в избранное";
            }
        }

        private async Task UpdateFilmInCollection(Film film)
        {
            await UpdateUI(() =>
            {
                var existing = Films.FirstOrDefault(f => f?.FilmId == film.FilmId);
                if (existing != null)
                {
                    existing.IsFavorite = film.IsFavorite;
                }
            });
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                Debug.WriteLine("Refreshing data...");
                await _filmsCacheService.ClearCacheAsync();
                await InitializeAsync();
                Debug.WriteLine("Data refreshed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Refresh error: {ex}");
                ErrorMessage = "Ошибка обновления данных";
            }
        }

        public void OnNavigatedTo(object parameter)
        {
            NavigationMessage = parameter switch
            {
                null => "Добро пожаловать!",
                string message => message,
                int filmId => $"Фильм ID: {filmId}",
                Film film => $"Просмотр: {film?.NameRu}",
                Dictionary<string, object> filters => ProcessFiltersMessage(filters),
                _ => "Неизвестный параметр"
            };

            Debug.WriteLine($"Navigation message: {NavigationMessage}");
        }

        private string ProcessFiltersMessage(Dictionary<string, object> filters)
        {
            var parts = new List<string>();

            if (filters.TryGetValue("genre", out var genre) && genre is string g)
                parts.Add($"Жанр: {g}");

            if (filters.TryGetValue("country", out var country) && country is string c)
                parts.Add($"Страна: {c}");

            if (filters.TryGetValue("year", out var year))
                parts.Add($"Год: {year}");

            return string.Join(" | ", parts);
        }

        private async Task UpdateUI(Action action)
        {
            try
            {
                if (CoreApplication.MainView?.Dispatcher != null)
                {
                    await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            action?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"UI action error: {ex}");
                        }
                    });
                }
                else
                {
                    action?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dispatcher error: {ex}");
            }
        }
    }
}