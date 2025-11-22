using AutoMarket.Models;
using AutoMarket.Popups;           // Потрібно для доступу до BrandFilterPopup і ModelFilterPopup
using AutoMarket.Services;
using CommunityToolkit.Maui.Views; // Потрібно для ShowPopupAsync
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoMarket.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        // --- Списки даних для вибору ---
        public ObservableCollection<VehicleTypeDto> VehicleTypes { get; set; } = new();
        public ObservableCollection<VehicleBrandDto> Brands { get; set; } = new();
        public ObservableCollection<VehicleModelDto> Models { get; set; } = new();
        public ObservableCollection<RegionDto> Regions { get; set; } = new();
        public ObservableCollection<CityDto> Cities { get; set; } = new();
        public ObservableCollection<FuelTypeDto> FuelTypes { get; set; } = new();
        public ObservableCollection<GearTypeDto> GearTypes { get; set; } = new();

        // --- Обрані значення (Selected Items) ---

        private VehicleTypeDto _selectedType;
        public VehicleTypeDto SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedTypeName));

                    // ВИПРАВЛЕННЯ: Перевіряємо на null перед тим, як брати Id
                    if (value != null)
                    {
                        LoadBrands(value.Id);
                    }
                    else
                    {
                        // Якщо тип скинули (null), то і список брендів треба очистити
                        Brands.Clear();
                    }
                }
            }
        }

        private VehicleBrandDto _selectedBrand;
        public VehicleBrandDto SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (_selectedBrand != value)
                {
                    _selectedBrand = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedBrandName));

                    // Логіка: Нова марка -> очищаємо модель -> вантажимо нові моделі
                    if (value != null) LoadModels(value.Id);
                }
            }
        }

        private VehicleModelDto _selectedModel;
        public VehicleModelDto SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedModelName));
            }
        }

        private RegionDto _selectedRegion;
        public RegionDto SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (_selectedRegion != value)
                {
                    _selectedRegion = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedRegionName));

                    if (value != null) LoadCities(value.Id);
                }
            }
        }

        private CityDto _selectedCity;
        public CityDto SelectedCity
        {
            get => _selectedCity;
            set
            {
                _selectedCity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedCityName));
            }
        }

        // --- Властивості для відображення тексту ---
        public string SelectedTypeName => SelectedType?.Name ?? "Тип транспорту";
        public string SelectedBrandName => SelectedBrand?.Name ?? "Марка";
        public string SelectedModelName => SelectedModel?.Name ?? "Модель";
        public string SelectedRegionName => SelectedRegion?.Name ?? "Регіон";
        public string SelectedCityName => SelectedCity?.Name ?? "Місто";
        private int? _yearFrom;
        public int? YearFrom
        {
            get => _yearFrom;
            set
            {
                _yearFrom = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedYearText)); // Оновлюємо текст
            }
        }

        private int? _yearTo;
        public int? YearTo
        {
            get => _yearTo;
            set
            {
                _yearTo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedYearText)); // Оновлюємо текст
            }
        }

        // Текст для відображення на кнопці
        public string SelectedYearText
        {
            get
            {
                if (YearFrom == null && YearTo == null) return "Рік випуску";
                if (YearFrom != null && YearTo == null) return $"від {YearFrom}";
                if (YearFrom == null && YearTo != null) return $"до {YearTo}";
                return $"{YearFrom} - {YearTo}";
            }
        }

        // Ціна(зберігаємо завжди в USD)
        private int? _priceFrom;
        private int? _priceTo;

        // Текст для відображення на кнопці
        public string SelectedPriceText
        {
            get
            {
                if (_priceFrom == null && _priceTo == null) return "Вартість";
                // Показуємо знак $, бо ми все перевели в долари
                if (_priceFrom != null && _priceTo == null) return $"від {_priceFrom} $";
                if (_priceFrom == null && _priceTo != null) return $"до {_priceTo} $";
                return $"{_priceFrom} - {_priceTo} $";
            }
        }

        private FuelTypeDto _selectedFuel;
        public FuelTypeDto SelectedFuel
        {
            get => _selectedFuel;
            set
            {
                _selectedFuel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedFuelName));
            }
        }

        private GearTypeDto _selectedGear;
        public GearTypeDto SelectedGear
        {
            get => _selectedGear;
            set
            {
                _selectedGear = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedGearName));
            }
        }

        public string SelectedGearName => SelectedGear?.Name ?? "КПП";

        public string SelectedFuelName => SelectedFuel?.Name ?? "Пальне";

        // --- Команди ---
        public ICommand SelectTypeCommand { get; }
        public ICommand SelectBrandCommand { get; } // Додано
        public ICommand SelectModelCommand { get; } // Додано
        public ICommand SearchCommand { get; }
        public ICommand SelectYearCommand { get; }
        public ICommand SelectPriceCommand { get; }
        public ICommand SelectRegionCommand { get; }
        public ICommand SelectCityCommand { get; }
        public ICommand SelectFuelCommand { get; }
        public ICommand SelectGearCommand { get; }
        public ICommand ResetFiltersCommand { get; }

        // --- КОНСТРУКТОР ---
        public MainPageViewModel()
        {
            _apiService = new ApiService();

            // Ініціалізація команд
            SelectTypeCommand = new Command(OnSelectType); // Тип
            SelectBrandCommand = new Command(OnSelectBrand); // Бренд
            SelectModelCommand = new Command(OnSelectModel); // Модель
            SelectYearCommand = new Command(OnSelectYear); // Рік
            SelectPriceCommand = new Command(OnSelectPrice); // Ціна
            SelectRegionCommand = new Command(OnSelectRegion); // Регіон
            SelectCityCommand = new Command(OnSelectCity);// Місто
            SelectFuelCommand = new Command(OnSelectFuel); // Тип Палива
            SelectGearCommand = new Command(OnSelectGear); // Коробка
            ResetFiltersCommand = new Command(OnResetFilters);
            SearchCommand = new Command(OnSearch);

            LoadInitialData();
        }

        // --- Методи логіки (Commands Implementation) ---

        private async void OnSelectType()
        {
            if (VehicleTypes == null || VehicleTypes.Count == 0)
            {
                // Пробуємо завантажити ще раз, якщо список порожній
                await LoadInitialDataAsync();
                if (VehicleTypes.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Помилка", "Список типів порожній", "ОК");
                    return;
                }
            }

            var popup = new VehicleTypeFilterPopup(VehicleTypes.ToList());
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            if (result is VehicleTypeDto selectedType)
            {
                SelectedType = selectedType;
            }
        }

        private async void OnSelectBrand()
        {
            // Перевірка залежності
            if (SelectedType == null)
            {
                await App.Current.MainPage.DisplayAlert("Увага", "Спочатку оберіть Тип транспорту!", "ОК");
                return;
            }

            if (Brands.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert("Інфо", "Список марок завантажується або порожній", "ОК");
                // Можна спробувати перезавантажити: await LoadBrands(SelectedType.Id);
                return;
            }

            // Відкриваємо Popup марок
            var popup = new BrandFilterPopup(Brands.ToList());
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            if (result is VehicleBrandDto selectedBrand)
            {
                SelectedBrand = selectedBrand;
            }
        }

        private async void OnSelectModel()
        {
            // Перевірка залежності
            if (SelectedBrand == null)
            {
                await App.Current.MainPage.DisplayAlert("Увага", "Спочатку оберіть Марку!", "ОК");
                return;
            }

            if (Models.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert("Інфо", "Список моделей порожній", "ОК");
                return;
            }

            // Відкриваємо Popup моделей
            var popup = new ModelFilterPopup(Models.ToList());
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            if (result is VehicleModelDto selectedModel)
            {
                SelectedModel = selectedModel;
            }
        }

        private async void OnSelectYear()
        {
            var popup = new YearFilterPopup();

            // Чекаємо результат
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            // --- ВИПРАВЛЕННЯ ---
            // Замість складного паттерну, використовуємо явну перевірку на ValueTuple
            if (result is ValueTuple<int?, int?> range)
            {
                // range.Item1 - це "від"
                // range.Item2 - це "до"
                YearFrom = range.Item1;
                YearTo = range.Item2;
            }
        }

        private async void OnSelectPrice()
        {
            var popup = new PriceFilterPopup();
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            if (result is ValueTuple<int?, int?> range)
            {
                _priceFrom = range.Item1;
                _priceTo = range.Item2;

                OnPropertyChanged(nameof(SelectedPriceText));

                // Для діагностики
                Console.WriteLine($"Обрано ціну: {_priceFrom} - {_priceTo} USD");
            }
        }

        private async void OnSelectRegion()
        {
            // Перевіряємо, чи список завантажився
            if (Regions.Count == 0)
            {
                // Якщо пусто, спробуємо перезавантажити
                var regions = await _apiService.GetRegionsAsync();
                ClearAndAdd(Regions, regions);

                if (Regions.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Інфо", "Список регіонів порожній", "ОК");
                    return;
                }
            }

            // Відкриваємо Popup
            var popup = new RegionFilterPopup(Regions.ToList());
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            if (result is RegionDto selectedRegion)
            {
                SelectedRegion = selectedRegion;

                // ВАЖЛИВО: Коли змінили регіон - старе місто вже не актуальне
                SelectedCity = null;
                Cities.Clear();

                // Вантажимо міста для цього регіону
                LoadCities(selectedRegion.Id);
            }
        }

        private async void OnSelectCity()
        {
            // 1. Перевірка: Чи обрано Регіон?
            if (SelectedRegion == null)
            {
                await App.Current.MainPage.DisplayAlert("Увага", "Спочатку оберіть Область (Регіон)!", "ОК");
                return;
            }

            // 2. Перевірка: Чи є список міст?
            if (Cities.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert("Інфо", "Список міст завантажується або порожній для цього регіону", "ОК");
                return;
            }

            // 3. Відкриваємо Popup
            var popup = new CityFilterPopup(Cities.ToList());
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            if (result is CityDto selectedCity)
            {
                SelectedCity = selectedCity;
            }
        }

        private async void OnSelectFuel()
        {
            // Якщо список порожній - спробуємо довантажити
            if (FuelTypes.Count == 0)
            {
                var fuels = await _apiService.GetFuelTypesAsync();
                ClearAndAdd(FuelTypes, fuels);

                if (FuelTypes.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Інфо", "Список пального порожній", "ОК");
                    return;
                }
            }

            var popup = new FuelFilterPopup(FuelTypes.ToList());
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            if (result is FuelTypeDto selectedFuel)
            {
                SelectedFuel = selectedFuel;
            }
        }

        private async void OnSelectGear()
        {
            if (GearTypes.Count == 0)
            {
                var gears = await _apiService.GetGearTypesAsync();
                ClearAndAdd(GearTypes, gears);

                if (GearTypes.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Інфо", "Список КПП порожній", "ОК");
                    return;
                }
            }

            var popup = new GearFilterPopup(GearTypes.ToList());
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            if (result is GearTypeDto selectedGear)
            {
                SelectedGear = selectedGear;
            }
        }

        private void OnResetFilters()
        {
            // 1. Скидаємо головні категорії
            SelectedType = null;
            SelectedRegion = null;
            SelectedFuel = null;
            SelectedGear = null;

            // 2. Скидаємо залежні категорії і чистимо їх списки
            SelectedBrand = null;
            Brands.Clear(); // Щоб при наступному відкритті не висіли старі марки

            SelectedModel = null;
            Models.Clear();

            SelectedCity = null;
            Cities.Clear();

            // 3. Скидаємо діапазони (Рік і Ціна)
            YearFrom = null;
            YearTo = null;
            _priceFrom = null;
            _priceTo = null;

            // 4. Оновлюємо текст на кнопках (Рік і Ціна), бо вони не оновлюються автоматично через null
            OnPropertyChanged(nameof(SelectedYearText));
            OnPropertyChanged(nameof(SelectedPriceText));

            // 5. Одразу запускаємо пошук, щоб показати ВСІ авто (скинути список)
            OnSearch();
        }

        private void OnSearch()
        {
            // Починаємо з повного списку
            var filtered = _allListingsCache.AsEnumerable();

            // 1. Фільтр Типу
            if (SelectedType != null)
            {
                // Припускаємо, що у авто є TypeId або ми не фільтруємо тип, якщо його нема в моделі.
                // Якщо в CarListing немає поля TypeId, цей крок можна пропустити або додати поле.
            }

            // 2. Марка (Brand)
            if (SelectedBrand != null)
            {
                filtered = filtered.Where(x => x.BrandObj != null && x.BrandObj.Id == SelectedBrand.Id);
            }

            // 3. Модель
            if (SelectedModel != null)
            {
                filtered = filtered.Where(x => x.ModelObj != null && x.ModelObj.Id == SelectedModel.Id);
            }

            // 4. Рік (Від / До)
            if (YearFrom != null)
            {
                filtered = filtered.Where(x => x.Year >= YearFrom);
            }
            if (YearTo != null)
            {
                filtered = filtered.Where(x => x.Year <= YearTo);
            }

            // 5. Ціна (Від / До) - пам'ятаємо, що в базі все в USD
            if (_priceFrom != null)
            {
                filtered = filtered.Where(x => x.Price >= _priceFrom);
            }
            if (_priceTo != null)
            {
                filtered = filtered.Where(x => x.Price <= _priceTo);
            }

            // 6. Регіон (Область) - фільтруємо по місту, бо авто прив'язане до міста
            // Але в CarListing.CityObj ми маємо тільки місто.
            // Якщо треба фільтрувати по регіону, нам треба знати RegionId міста авто.
            // Поки пропустимо регіон, якщо в моделі авто немає RegionId, і фільтруватимемо лише якщо обрано конкретне Місто.

            // 7. Місто
            if (SelectedCity != null)
            {
                filtered = filtered.Where(x => x.CityObj != null && x.CityObj.Id == SelectedCity.Id);
            }

            // 8. Пальне
            if (SelectedFuel != null)
            {
                filtered = filtered.Where(x => x.FuelObj != null && x.FuelObj.Id == SelectedFuel.Id);
            }

            // 9. КПП (Gear)
            if (SelectedGear != null)
            {
                filtered = filtered.Where(x => x.GearObj != null && x.GearObj.Id == SelectedGear.Id);
            }

            // Перетворюємо результат назад у список і оновлюємо екран
            var finalResult = filtered.ToList();

            if (finalResult.Count == 0)
            {
                App.Current.MainPage.DisplayAlert("Пошук", "За вашими фільтрами нічого не знайдено", "ОК");
            }

            ClearAndAdd(Listings, finalResult);
        }

        // --- Завантаження даних (API Calls) ---

        private async void LoadInitialData()
        {
            await LoadInitialDataAsync();
        }

        public ObservableCollection<CarListing> Listings { get; set; } = new();
        private async Task LoadInitialDataAsync()
        {
            try
            {
                var types = await _apiService.GetVehicleTypesAsync();
                ClearAndAdd(VehicleTypes, types);

                var regions = await _apiService.GetRegionsAsync();
                ClearAndAdd(Regions, regions);

                // --- ЗМІНА ТУТ ---
                var cars = await _apiService.GetAllListingsAsync();

                _allListingsCache = cars; // 1. Зберігаємо в кеш
                ClearAndAdd(Listings, cars); // 2. Показуємо на екрані
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження: {ex.Message}");
            }
        }

        private async void LoadBrands(int typeId)
        {
            try
            {
                Brands.Clear();
                SelectedBrand = null; // Скидаємо вибір марки при зміні типу
                SelectedModel = null; // Скидаємо вибір моделі

                var brands = await _apiService.GetBrandsByTypeAsync(typeId);
                ClearAndAdd(Brands, brands);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private async void LoadModels(int brandId)
        {
            try
            {
                Models.Clear();
                SelectedModel = null; // Скидаємо вибір моделі при зміні марки

                var models = await _apiService.GetModelsByBrandAsync(brandId);
                ClearAndAdd(Models, models);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private async void LoadCities(int regionId)
        {
            try
            {
                Cities.Clear();
                SelectedCity = null;

                var cities = await _apiService.GetCitiesByRegionAsync(regionId);
                ClearAndAdd(Cities, cities);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        private List<CarListing> _allListingsCache = new();
        private void ClearAndAdd<T>(ObservableCollection<T> collection, List<T> newItems)
        {
            collection.Clear();
            if (newItems != null)
            {
                foreach (var item in newItems) collection.Add(item);
            }
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        

    }
}