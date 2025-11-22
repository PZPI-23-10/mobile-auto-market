using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AutoMarket.Services;
using AutoMarket.Models;
using CommunityToolkit.Maui.Views;
using AutoMarket.Popups;

namespace AutoMarket.ViewModel
{
    public class AddListingViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        // --- 1. КОЛЕКЦІЯ ФОТОГРАФІЙ ---
        public ObservableCollection<ImageSource> DisplayPhotos { get; set; } = new();
        private List<FileResult> _filesToSend = new();

        // --- Властивості ---
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
                    SelectedBrand = null;
                    SelectedModel = null;
                    if (value != null) LoadBrands(value.Id);
                }
            }
        }
        public string SelectedTypeName => SelectedType?.Name ?? "Оберіть";

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
                    SelectedModel = null;
                    if (value != null) LoadModels(value.Id);
                }
            }
        }
        public string SelectedBrandName => SelectedBrand?.Name ?? "Оберіть";

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
        public string SelectedModelName => SelectedModel?.Name ?? "Оберіть";

        private int? _selectedYear;
        public int? SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedYearName));
            }
        }
        public string SelectedYearName => SelectedYear?.ToString() ?? "Оберіть";

        private string _mileage;
        public string Mileage
        {
            get => _mileage;
            set { _mileage = value; OnPropertyChanged(); }
        }

        private BaseDto _selectedBodyType;
        public BaseDto SelectedBodyType
        {
            get => _selectedBodyType;
            set
            {
                _selectedBodyType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedBodyTypeName));
            }
        }
        public string SelectedBodyTypeName => SelectedBodyType?.Name ?? "Оберіть";

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
                    SelectedCity = null;
                    if (value != null) LoadCities(value.Id);
                }
            }
        }
        public string SelectedRegionName => SelectedRegion?.Name ?? "Оберіть";

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
        public string SelectedCityName => SelectedCity?.Name ?? "Оберіть";

        // КРОК 3: Опис
        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        // КРОК 4: Характеристики
        private List<ColorDto> _availableColors;
        private ColorDto _selectedColor;
        public ColorDto SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedColorName));
                OnPropertyChanged(nameof(SelectedColorValue));
            }
        }
        public string SelectedColorName => SelectedColor?.Name ?? "Оберіть колір";
        public Color SelectedColorValue => SelectedColor?.ColorValue ?? Colors.Transparent;

        private bool _hasAccident;
        public bool HasAccident
        {
            get => _hasAccident;
            set { _hasAccident = value; OnPropertyChanged(); }
        }

        private BaseDto _selectedCondition;
        public BaseDto SelectedCondition
        {
            get => _selectedCondition;
            set
            {
                _selectedCondition = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedConditionName));
            }
        }
        public string SelectedConditionName => SelectedCondition?.Name ?? "Оберіть стан";

        private string _licensePlate;
        public string LicensePlate
        {
            get => _licensePlate;
            set { _licensePlate = value; OnPropertyChanged(); }
        }

        // ПАЛЬНЕ
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
        public string SelectedFuelName => SelectedFuel?.Name ?? "Оберіть пальне";

        // КПП (Коробка передач)
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
        public string SelectedGearName => SelectedGear?.Name ?? "Оберіть КПП";

        // ===============================
        //      КРОК 5: ВАРТІСТЬ
        // ===============================

        // 1. Ціна, яку вводить користувач (зберігаємо як рядок, щоб не було проблем з пустим полем)
        private string _inputPrice;
        public string InputPrice
        {
            get => _inputPrice;
            set { _inputPrice = value; OnPropertyChanged(); }
        }

        // 2. Список доступних валют для Picker-а
        public List<string> Currencies { get; } = new List<string> { "$", "₴", "€" };

        // 3. Обрана валюта (за замовчуванням Долар)
        private string _selectedCurrency = "$";
        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set { _selectedCurrency = value; OnPropertyChanged(); }
        }

        // --- Допоміжний метод для конвертації (знадобиться при відправці) ---
        public double GetFinalPriceInUsd()
        {
            if (double.TryParse(InputPrice, out double price))
            {
                switch (SelectedCurrency)
                {
                    case "₴": return price / 41.5; // Курс грн -> долар
                    case "€": return price * 1.05; // Курс євро -> долар
                    default: return price;         // Вже в доларах
                }
            }
            return 0;
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                // Оновлюємо доступність кнопки
                ((Command)SubmitCommand).ChangeCanExecute();
            }
        }

        // --- КОМАНДИ ---
        public ICommand AddPhotoCommand { get; }
        public ICommand RemovePhotoCommand { get; }
        public ICommand SelectTypeCommand { get; }
        public ICommand SelectBrandCommand { get; }
        public ICommand SelectModelCommand { get; }
        public ICommand SelectYearCommand { get; }
        public ICommand SelectBodyTypeCommand { get; }
        public ICommand SelectRegionCommand { get; }
        public ICommand SelectCityCommand { get; }
        public ICommand SelectColorCommand { get; }
        public ICommand SelectConditionCommand { get; }
        public ICommand SelectFuelCommand { get; }
        public ICommand SelectGearCommand { get; }
        public ICommand SubmitCommand { get; }

        public AddListingViewModel(ApiService apiService)
        {
            _apiService = apiService;

            AddPhotoCommand = new Command(OnAddPhoto);
            RemovePhotoCommand = new Command<ImageSource>(OnRemovePhoto);

            SelectTypeCommand = new Command(OnSelectType);
            SelectBrandCommand = new Command(OnSelectBrand);
            SelectModelCommand = new Command(OnSelectModel);
            SelectYearCommand = new Command(OnSelectYear);
            SelectBodyTypeCommand = new Command(OnSelectBodyType);
            SelectRegionCommand = new Command(OnSelectRegion);
            SelectCityCommand = new Command(OnSelectCity);
            SelectColorCommand = new Command(OnSelectColor);
            SelectConditionCommand = new Command(OnSelectCondition);
            SelectFuelCommand = new Command(OnSelectFuel);
            SelectGearCommand = new Command(OnSelectGear);
            SubmitCommand = new Command(OnSubmit, () => !IsBusy);

            _availableColors = new List<ColorDto>
            {
                new ColorDto { Name = "Чорний", HexCode = "#000000" },
                new ColorDto { Name = "Білий", HexCode = "#FFFFFF" },
                new ColorDto { Name = "Сірий", HexCode = "#808080" },
                new ColorDto { Name = "Срібний", HexCode = "#C0C0C0" },
                new ColorDto { Name = "Червоний", HexCode = "#FF0000" },
                new ColorDto { Name = "Синій", HexCode = "#0000FF" },
                new ColorDto { Name = "Зелений", HexCode = "#008000" },
                new ColorDto { Name = "Коричневий", HexCode = "#A52A2A" },
                new ColorDto { Name = "Бежевий", HexCode = "#F5F5DC" },
                new ColorDto { Name = "Жовтий", HexCode = "#FFFF00" }
            };
        }

        // --- Логіка ---
        private async void OnAddPhoto()
        {
            try
            {
                var results = await FilePicker.PickMultipleAsync(new PickOptions
                {
                    PickerTitle = "Оберіть фото авто",
                    FileTypes = FilePickerFileType.Images
                });

                if (results != null)
                {
                    foreach (var file in results)
                    {
                        _filesToSend.Add(file);
                        var stream = await file.OpenReadAsync();
                        DisplayPhotos.Add(ImageSource.FromStream(() => stream));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка вибору фото: {ex.Message}");
            }
        }

        private void OnRemovePhoto(ImageSource photo)
        {
            if (DisplayPhotos.Contains(photo)) DisplayPhotos.Remove(photo);
        }

        public List<FileResult> GetFilesToSend() => _filesToSend;

        private async void OnSelectType()
        {
            var items = await _apiService.GetVehicleTypesAsync();
            var popup = new VehicleTypeFilterPopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is VehicleTypeDto item) SelectedType = item;
        }

        private async void LoadBrands(int typeId) { }
        private async void OnSelectBrand()
        {
            if (SelectedType == null) { await App.Current.MainPage.DisplayAlert("Увага", "Оберіть тип транспорту", "ОК"); return; }
            var items = await _apiService.GetBrandsByTypeAsync(SelectedType.Id);
            var popup = new BrandFilterPopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is VehicleBrandDto item) SelectedBrand = item;
        }

        private async void LoadModels(int brandId) { }
        private async void OnSelectModel()
        {
            if (SelectedBrand == null) { await App.Current.MainPage.DisplayAlert("Увага", "Оберіть марку", "ОК"); return; }
            var items = await _apiService.GetModelsByBrandAsync(SelectedBrand.Id);
            var popup = new ModelFilterPopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is VehicleModelDto item) SelectedModel = item;
        }

        private async void OnSelectYear()
        {
            var popup = new SingleYearPopup();
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is int year) SelectedYear = year;
        }

        private async void OnSelectBodyType()
        {
            var items = await _apiService.GetBodyTypesAsync();
            var popup = new BodyTypePopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is BaseDto item) SelectedBodyType = item;
        }

        private async void OnSelectRegion()
        {
            var items = await _apiService.GetRegionsAsync();
            var popup = new RegionFilterPopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is RegionDto item) SelectedRegion = item;
        }

        private async void LoadCities(int regionId) { }
        private async void OnSelectCity()
        {
            if (SelectedRegion == null) { await App.Current.MainPage.DisplayAlert("Увага", "Оберіть регіон", "ОК"); return; }
            var items = await _apiService.GetCitiesByRegionAsync(SelectedRegion.Id);
            var popup = new CityFilterPopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is CityDto item) SelectedCity = item;
        }

        private async void OnSelectColor()
        {
            var popup = new ColorPopup(_availableColors);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is ColorDto item) SelectedColor = item;
        }

        private async void OnSelectCondition()
        {
            var items = await _apiService.GetConditionsAsync();
            if (items.Count == 0) { await App.Current.MainPage.DisplayAlert("Інфо", "Список пустий", "ОК"); return; }
            var popup = new ConditionPopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);
            if (result is BaseDto item) SelectedCondition = item;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void OnSelectFuel()
        {
            // 1. Вантажимо список
            var items = await _apiService.GetFuelTypesAsync();
            if (items.Count == 0) { await App.Current.MainPage.DisplayAlert("Інфо", "Список пального порожній", "ОК"); return; }

            // 2. Відкриваємо Popup (той самий, що на Головній)
            var popup = new FuelFilterPopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            // 3. Зберігаємо результат
            if (result is FuelTypeDto item) SelectedFuel = item;
        }

        private async void OnSelectGear()
        {
            var items = await _apiService.GetGearTypesAsync();
            if (items.Count == 0) { await App.Current.MainPage.DisplayAlert("Інфо", "Список КПП порожній", "ОК"); return; }

            var popup = new GearFilterPopup(items);
            var result = await App.Current.MainPage.ShowPopupAsync(popup);

            if (result is GearTypeDto item) SelectedGear = item;
        }

        private async void OnSubmit()
        {
            if (IsBusy) return;

            // 1. ПЕРЕВІРКА (Додали Fuel і Gear)
            if (SelectedModel == null || SelectedBodyType == null || SelectedYear == null ||
                SelectedType == null || SelectedBrand == null || SelectedRegion == null ||
                SelectedCity == null || SelectedFuel == null || SelectedGear == null) // <--- ДОДАНО
            {
                await App.Current.MainPage.DisplayAlert("Помилка", "Заповніть всі обов'язкові поля!", "ОК");
                return;
            }

            if (string.IsNullOrWhiteSpace(InputPrice))
            {
                await App.Current.MainPage.DisplayAlert("Помилка", "Вкажіть ціну!", "ОК");
                return;
            }

            if (_filesToSend.Count == 0)
            {
                bool answer = await App.Current.MainPage.DisplayAlert("Фото", "Ви не додали жодного фото. Продовжити?", "Так", "Ні");
                if (!answer) return;
            }

            // 2. ПІДГОТОВКА ДАНИХ
            IsBusy = true;

            try
            {
                // Конвертуємо пробіг (якщо пустий - 0)
                int.TryParse(Mileage, out int mileageVal);

                // Конвертуємо ціну в USD
                double priceUsd = GetFinalPriceInUsd();

                // Отримуємо ID вибраних елементів
                // Увага: Для GearType, FuelType - ми їх не додали на формі, 
                // тому я поки поставлю 1 (або треба додати і їх вибір, як ми робили з кольором)
                // АЛЕ! У тебе в дизайні немає вибору Пального і КПП на сторінці додавання.
                // ТРЕБА АБО ДОДАТИ ЇХ (як колір), АБО ВІДПРАВЛЯТИ ЗАГЛУШКИ.
                // Я відправлю заглушки (1), щоб не ламався запит.

                int gearId = 1; // Заглушка
                int fuelId = 1; // Заглушка

                bool success = await _apiService.CreateListingAsync(
                    modelId: SelectedModel.Id,
                    bodyTypeId: SelectedBodyType.Id,

                    // ТУТ ТЕПЕР РЕАЛЬНІ ДАНІ
                    gearTypeId: SelectedGear.Id,
                    fuelTypeId: SelectedFuel.Id,

                    conditionId: SelectedCondition?.Id ?? 1,
                    cityId: SelectedCity.Id,
                    year: SelectedYear.Value,
                    mileage: mileageVal,
                    number: LicensePlate,
                    colorHex: SelectedColor?.HexCode ?? "#000000",
                    price: priceUsd,
                    description: Description,
                    hasAccident: HasAccident,
                    photos: _filesToSend
                );

                IsBusy = false;

                if (success)
                {
                    await App.Current.MainPage.DisplayAlert("Успіх", "Оголошення успішно створено!", "ОК");
                    // Повертаємось назад на головну
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Помилка", "Не вдалося створити оголошення. Перевірте інтернет.", "ОК");
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await App.Current.MainPage.DisplayAlert("Критична помилка", ex.Message, "ОК");
            }
        }
    }
}