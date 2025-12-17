using AutoMarket.Models;
using AutoMarket.Popups;           // Потрібно для доступу до BrandFilterPopup і ModelFilterPopup
using AutoMarket.Services;
using CommunityToolkit.Maui.Views; // Потрібно для ShowPopupAsync
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using AutoMarket.Messages;

namespace AutoMarket.ViewModel
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        private readonly Dictionary<string, string> _typeTranslations = new()
        {
            { "PASSENGER_CAR", "Легкові" },
            { "TRUCK", "Вантажівки" },
            { "MOTORCYCLE", "Мотоцикли" },
            { "BUS", "Автобуси" },
            { "TRAILER", "Причепи" },
            { "SPECIAL", "Спецтехніка" }
            // Додай сюди інші типи, які є в твоїй БД
        };
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
        // Ми беремо переклад напряму з менеджера. 
        // При перезапуску AppShell тут буде вже нова мова.

        public string SelectedTypeName
        {
            get
            {
                // Якщо нічого не обрано - показуємо стандартний текст "Тип транспорту"
                if (SelectedType == null)
                    return LocalizationManager.Instance["Filter_TransportType"];

                // Спробуємо знайти переклад у словнику
                if (_typeTranslations.TryGetValue(SelectedType.Name, out string translatedName))
                {
                    return translatedName; // Повертаємо "Легкові"
                }

                // Якщо перекладу немає - повертаємо як є ("PASSENGER_CAR")
                return SelectedType.Name;
            }
        }
        public string SelectedBrandName => SelectedBrand?.Name ?? LocalizationManager.Instance["Filter_Brand"];
        public string SelectedModelName => SelectedModel?.Name ?? LocalizationManager.Instance["Filter_Model"];
        public string SelectedRegionName => SelectedRegion?.Name ?? LocalizationManager.Instance["Filter_Region"];
        public string SelectedCityName => SelectedCity?.Name ?? LocalizationManager.Instance["Filter_City"];
        public string SelectedFuelName => SelectedFuel?.Name ?? LocalizationManager.Instance["Filter_Fuel"];
        public string SelectedGearName => SelectedGear?.Name ?? LocalizationManager.Instance["Filter_Gearbox"];

        // Для Року і Ціни логіка трохи складніша, тому залишаємо як є, 
        // АЛЕ треба додати оновлення тексту при ініціалізації.
        // Див. нижче про метод InitializeLocalizedText.

        // --- Рік та Ціна (Тут трохи складніше, бо це get-властивості) ---

        private int? _yearFrom;
        public int? YearFrom
        {
            get => _yearFrom;
            set
            {
                _yearFrom = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedYearText));
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
                OnPropertyChanged(nameof(SelectedYearText));
            }
        }

        public string SelectedYearText
        {
            get
            {
                // ЗАМІНА: "Рік випуску" -> LocalizationManager...
                if (YearFrom == null && YearTo == null) return LocalizationManager.Instance["Filter_Year"];

                if (YearFrom != null && YearTo == null) return $"від {YearFrom}";
                if (YearFrom == null && YearTo != null) return $"до {YearTo}";
                return $"{YearFrom} - {YearTo}";
            }
        }

        // Ціна
        private int? _priceFrom;
        private int? _priceTo;

        public string SelectedPriceText
        {
            get
            {
                // ЗАМІНА: "Вартість" -> LocalizationManager...
                if (_priceFrom == null && _priceTo == null) return LocalizationManager.Instance["Filter_Price"];

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

        // --- Команди ---
        public ICommand FilterConditionCommand { get; }
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
        public ICommand OpenDetailsCommand { get; }

        // --- КОНСТРУКТОР ---
        public MainPageViewModel(ApiService apiService)
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
            OpenDetailsCommand = new Command<CarListing>(OnOpenDetails);
            FilterConditionCommand = new Command<string>(ApplyConditionFilter);

            OnPropertyChanged(nameof(SelectedYearText));
            OnPropertyChanged(nameof(SelectedPriceText));

            LoadInitialData();

            WeakReferenceMessenger.Default.Register<FavoriteChangeMessage>(this, (recipient, message) =>
            {
                // Шукаємо, чи є така машина у нас на Головній
                var carOnMainPage = Listings.FirstOrDefault(x => x.Id == message.CarId);

                if (carOnMainPage != null)
                {
                    // Якщо знайшли — міняємо їй серце
                    carOnMainPage.IsFavorite = message.IsFavorite;
                }
            });
        }

        // Метод:
        private async void OnOpenDetails(CarListing listing)
        {
            if (listing == null) return;

            // Передаємо об'єкт listing на нову сторінку через словник параметрів
            var navigationParameter = new Dictionary<string, object>
    {
        { "Listing", listing }
    };

            await Shell.Current.GoToAsync(nameof(CarDetailsPage), navigationParameter);
        }

        // --- Методи логіки (Commands Implementation) ---

        // --- МЕТОД ФІЛЬТРАЦІЇ ---
        // Цей метод викликається при натисканні кнопок Всі/Вживані/Нові
        private void ApplyConditionFilter(string filterType)
        {
            _currentFilter = filterType;

            // Просто оновлюємо візуал (кольори кнопок)
            OnPropertyChanged(nameof(FilterAllColor));
            OnPropertyChanged(nameof(FilterUsedColor));
            OnPropertyChanged(nameof(FilterNewColor));

            // ВАЖЛИВО: Ми прибрали звідси логіку Listings.Clear() і фільтрацію.
            // Тепер користувач натиснув кнопку, вона засвітилась, але список не змінився,
            // поки він не натисне "ШУКАТИ".
        }

        // 1. Кеш для зберігання всіх завантажених авто
        private List<CarListing> _allListingsCache = new List<CarListing>();

        // 2. Кольори кнопок (для UI)
        public Color FilterAllColor => _currentFilter == "All" ? Colors.White : Colors.Transparent;
        public Color FilterUsedColor => _currentFilter == "Used" ? Colors.White : Colors.Transparent;
        public Color FilterNewColor => _currentFilter == "New" ? Colors.White : Colors.Transparent;

        // Поточний фільтр
        private string _currentFilter = "All"; // "All", "Used", "New"


        private async void OnSelectType()
        {
            // 1. Перевірка: якщо список порожній, пробуємо завантажити
            if (VehicleTypes == null || VehicleTypes.Count == 0)
            {
                await LoadInitialDataAsync();
                if (VehicleTypes.Count == 0)
                {
                    // Можна додати повідомлення про помилку, якщо список все одно пустий
                    return;
                }
            }

            // 2. "Магія" для відображення:
            // Ми створюємо ТИМЧАСОВИЙ список копій DTO.
            // В ці копії ми записуємо українські назви замість англійських.
            var uiList = VehicleTypes.Select(vt => new VehicleTypeDto
            {
                Id = vt.Id, // ID зберігаємо обов'язково! По ньому будемо шукати оригінал.

                // Підміняємо ім'я: якщо є в словнику - беремо переклад, інакше - оригінал
                Name = _typeTranslations.ContainsKey(vt.Name)
                       ? _typeTranslations[vt.Name]
                       : vt.Name
            }).ToList();

            // 3. Відкриваємо ТВІЙ кастомний Popup з гарним списком
            var popup = new VehicleTypeFilterPopup(uiList);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            // 4. Обробка результату
            if (result is VehicleTypeDto tempSelected)
            {
                // У tempSelected зараз назва "Легкові", а нам для API треба "PASSENGER_CAR".
                // Тому ми беремо ID вибраного елемента і шукаємо ОРИГІНАЛ у головному списку.
                var originalDto = VehicleTypes.FirstOrDefault(x => x.Id == tempSelected.Id);

                if (originalDto != null)
                {
                    // Присвоюємо оригінальний об'єкт. 
                    // Властивість SelectedTypeName сама підтягне переклад для кнопки на екрані.
                    SelectedType = originalDto;
                }
            }
        }

        private async void OnSelectBrand()
        {
            // Перевірка залежності
            if (SelectedType == null)
            {
                // БУЛО: "Увага", "Спочатку оберіть Тип транспорту!", "ОК"
                // СТАЛО:
                await App.Current.MainPage.DisplayAlert(
                    LocalizationManager.Instance["Alert_Attention"],
                    LocalizationManager.Instance["Error_SelectTypeFirst"],
                    "OK");
                return;
            }

            if (Brands.Count == 0)
            {
                // БУЛО: "Інфо", "Список марок завантажується...", "ОК"
                // СТАЛО:
                await App.Current.MainPage.DisplayAlert(
                    LocalizationManager.Instance["Alert_Info"],
                    LocalizationManager.Instance["Error_ListEmpty_Brand"],
                    "OK");
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

        // У MainPageViewModel.cs

        private async void OnSelectModel()
        {
            if (SelectedBrand == null)
            {
                await App.Current.MainPage.DisplayAlert(
            LocalizationManager.Instance["Alert_Attention"],
            LocalizationManager.Instance["Error_SelectBrandFirst"],
            "OK");
                return;
            }

            // ПЕРЕВІРКА: Чи обрано Тип транспорту?
            // Якщо не обрано, можна передавати 0 або null, залежить від API.
            // Але логічно, що для фільтрації треба обрати тип.
            int typeId = SelectedType?.Id ?? 0;

            // ВИКЛИКАЄМО НОВИЙ МЕТОД З ДВОМА ПАРАМЕТРАМИ
            var items = await _apiService.GetModelsByBrandAsync(SelectedBrand.Id, typeId);

            var popup = new ModelFilterPopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is VehicleModelDto item) SelectedModel = item;
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
                    await App.Current.MainPage.DisplayAlert(
                LocalizationManager.Instance["Alert_Info"],
                LocalizationManager.Instance["Error_ListEmpty_Region"],
                "OK");
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
                await App.Current.MainPage.DisplayAlert(
            LocalizationManager.Instance["Alert_Attention"],
            LocalizationManager.Instance["Error_SelectRegionFirst"],
            "OK");
                return;
            }

            // 2. Перевірка: Чи є список міст?
            if (Cities.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert(
            LocalizationManager.Instance["Alert_Info"],
            LocalizationManager.Instance["Error_ListEmpty_City"],
            "OK");
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
                    await App.Current.MainPage.DisplayAlert(
                LocalizationManager.Instance["Alert_Info"],
                LocalizationManager.Instance["Error_ListEmpty_Fuel"],
                "OK");
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
                    await App.Current.MainPage.DisplayAlert(
                LocalizationManager.Instance["Alert_Info"],
                LocalizationManager.Instance["Error_ListEmpty_Gear"],
                "OK");
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

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(); // Повідомляємо інтерфейс про зміну
            }
        }

        private async void OnSearch()
        {
            // 1. Вмикаємо індикатор завантаження
            IsBusy = true;

            try
            {
                // 2. Беремо повний список з кешу (всі завантажені авто)
                var filtered = _allListingsCache.AsEnumerable();

                // ==========================================
                //      ЕТАП 1: ПЕРЕМИКАЧ (Всі/Вживані/Нові)
                // ==========================================
                // Тепер кнопка "Шукати" враховує, що натиснуто вгорі
                switch (_currentFilter)
                {
                    case "Used": // Вживані
                        filtered = filtered.Where(x => x.Mileage > 0);
                        break;
                    case "New": // Нові
                        filtered = filtered.Where(x => x.Mileage == 0);
                        break;
                        // case "All": нічого не робимо, показуємо все
                }

                // ==========================================
                //      ЕТАП 2: ФІЛЬТРИ З ВИПАДАЮЧИХ СПИСКІВ
                // ==========================================

                // 1. Тип транспорту
                if (SelectedType != null)
                {
                    // Перевіряємо, чи є таке поле в моделі. Зазвичай це VehicleType або схоже.
                    // Якщо поле називається інакше - підправ тут.
                    // filtered = filtered.Where(x => x.VehicleType?.Id == SelectedType.Id);
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

                // 5. Ціна (Від / До)
                if (_priceFrom != null)
                {
                    filtered = filtered.Where(x => x.Price >= _priceFrom);
                }
                if (_priceTo != null)
                {
                    filtered = filtered.Where(x => x.Price <= _priceTo);
                }

                // 6. Місто (Регіон пропускаємо, бо авто прив'язане до міста)
                if (SelectedCity != null)
                {
                    filtered = filtered.Where(x => x.CityObj != null && x.CityObj.Id == SelectedCity.Id);
                }

                // 7. Пальне
                if (SelectedFuel != null)
                {
                    filtered = filtered.Where(x => x.FuelObj != null && x.FuelObj.Id == SelectedFuel.Id);
                }

                // 8. КПП (Gear)
                if (SelectedGear != null)
                {
                    filtered = filtered.Where(x => x.GearObj != null && x.GearObj.Id == SelectedGear.Id);
                }

                // ==========================================
                //      ЕТАП 3: ОНОВЛЕННЯ ЕКРАНУ
                // ==========================================

                var finalResult = filtered.ToList();

                // Очищаємо список на екрані і додаємо відфільтровані
                Listings.Clear();
                foreach (var car in finalResult)
                {
                    Listings.Add(car);
                }

                // Якщо нічого не знайшли - кажемо про це
                if (finalResult.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert(
        LocalizationManager.Instance["Alert_Info"],
        LocalizationManager.Instance["Msg_NothingFound"],
        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка пошуку: {ex.Message}");
            }
            finally
            {
                // Вимикаємо індикатор
                IsBusy = false;
            }
        }

        // --- Завантаження даних (API Calls) ---

        private async void LoadInitialData()
        {
            await LoadInitialDataAsync();
        }

        public ObservableCollection<CarListing> Listings { get; set; } = new();
        private async Task LoadInitialDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
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
                await SyncFavoritesState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // 2. Метод синхронізації
        private async Task SyncFavoritesState()
        {
            // Отримуємо список улюблених з сервера
            var myFavorites = await _apiService.GetFavoriteListingsAsync();

            // Робимо список ID для швидкого пошуку
            var favIds = myFavorites.Select(f => f.Id).ToHashSet();

            // Проходимось по головному списку
            foreach (var car in Listings)
            {
                // Якщо ID машини є в улюблених — ставимо IsFavorite = true
                if (favIds.Contains(car.Id))
                {
                    car.IsFavorite = true;
                }
                else
                {
                    car.IsFavorite = false;
                }
            }
        }

        // 3. Команда натискання на серце (вона в тебе вже є, перевір її)
        [RelayCommand]
        private async Task ToggleFavorite(CarListing car)
        {
            if (car == null) return;

            // Миттєво міняємо колір (для швидкості UI)
            car.IsFavorite = !car.IsFavorite;

            bool success;
            if (car.IsFavorite)
                success = await _apiService.AddToFavoritesAsync(car.Id);
            else
                success = await _apiService.RemoveFromFavoritesAsync(car.Id);

            // Якщо сервер видав помилку — повертаємо все як було
            if (!success)
            {
                car.IsFavorite = !car.IsFavorite;
                // Можна показати Toast або Alert, але не обов'язково
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

                // 1. Беремо ID типу транспорту (якщо є)
                int typeId = SelectedType?.Id ?? 0;

                // 2. Передаємо ДВА параметри: brandId та typeId
                var models = await _apiService.GetModelsByBrandAsync(brandId, typeId);

                ClearAndAdd(Models, models);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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


        private void ClearAndAdd<T>(ObservableCollection<T> collection, List<T> newItems)
        {
            collection.Clear();
            if (newItems != null)
            {
                foreach (var item in newItems) collection.Add(item);
            }
        }

        // --- INotifyPropertyChanged ---
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}


    }
}