using CommunityToolkit.Mvvm.ComponentModel; // Для [ObservableProperty]
using CommunityToolkit.Mvvm.Input; // Для [RelayCommand]
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views; // Для ShowPopupAsync
using AutoMarket.Models;
using AutoMarket.Services;

namespace AutoMarket.ViewModels
{
    // Partial class + [ObservableObject] - це магія з MVVM Toolkit
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        // ==========================================================
        //     1. ВЛАСТИВОСТІ СТАНУ (STATE)
        // ==========================================================

        // [ObservableProperty] автоматично створює властивість 'Cars'
        // і сповіщає UI (MainPage) про будь-які зміни.
        [ObservableProperty]
        private ObservableCollection<CarListing> _cars;

        // Властивість для обраного фільтра пального
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FuelFilterText))] // Оновити текст кнопки
        private string _selectedFuel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VehicleTypeFilterText))]
        private VehicleType _selectedVehicleType;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MakeFilterText))] // <-- Змінено
        private Make _selectedMake;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ModelFilterText))] // <-- Змінено
        private Model _selectedModel;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TransmissionFilterText))]
        private string _selectedTransmission;

        [ObservableProperty]
        // Цей рядок автоматично оновить 6 властивостей стилю, які ми створимо нижче
        [NotifyPropertyChangedFor(nameof(AllButtonBg), nameof(AllButtonText), nameof(UsedButtonBg), nameof(UsedButtonText), nameof(NewButtonBg), nameof(NewButtonText))]
        private string _selectedCondition ;

        public string VehicleTypeFilterText => SelectedVehicleType?.Name ?? LocalizationManager.Instance["FilterVehicleType"];
        public string TransmissionFilterText => string.IsNullOrEmpty(SelectedTransmission)
        ? LocalizationManager.Instance["FilterTransmission"] // <-- ЗМІНЕНО
        : SelectedTransmission;

        // Властивість для кнопки "Марка"
        public string MakeFilterText => SelectedMake?.Name ?? LocalizationManager.Instance["FilterMake"];

        // Властивість для кнопки "Модель"
        public string ModelFilterText => SelectedModel?.Name?? LocalizationManager.Instance["FilterModel"]; // <-- ЗМІНЕНО

        // Стилі для кнопки "Всі"
        public Color AllButtonBg => SelectedCondition == "Всі" ? Colors.White : Colors.Transparent;
        public Color AllButtonText => SelectedCondition == "Всі" ? Colors.Black : Color.FromHex("#555");

        // Стилі для кнопки "Вживані"
        public Color UsedButtonBg => SelectedCondition == "Вживані" ? Colors.White : Colors.Transparent;
        public Color UsedButtonText => SelectedCondition == "Вживані" ? Colors.Black : Color.FromHex("#555");

        // Стилі для кнопки "Нові"
        public Color NewButtonBg => SelectedCondition == "Нові" ? Colors.White : Colors.Transparent;
        public Color NewButtonText => SelectedCondition == "Нові" ? Colors.Black : Color.FromHex("#555");

        // Це "обчислена" властивість для тексту на кнопці
        public string FuelFilterText => string.IsNullOrEmpty(SelectedFuel)
            ? LocalizationManager.Instance["FuelFilterText"] 
            : SelectedFuel;

        [ObservableProperty]
        private bool _isLoading = false; // Для індикатора завантаження

        // ==========================================================
        //     2. КОНСТРУКТОР
        // ==========================================================
        // Ми "запитуємо" ApiService, і MAUI нам його передасть (це називається Dependency Injection)
        public MainPageViewModel(ApiService apiService)
        {
            _apiService = apiService;
            _cars = new ObservableCollection<CarListing>();

            // Завантажуємо дані при першому запуску
            // (Ми не можемо викликати async метод в конструкторі,
            // тому робимо це "неочікувано". Це нормально для VM.)
            Task.Run(async () => await LoadDataAsync());

            LocalizationManager.Instance.PropertyChanged += (s, e) => OnLanguageChanged();
        }

        // ==========================================================
        //     3. КОМАНДИ (COMMANDS) - те, що XAML викликає
        // ==========================================================

        // [RelayCommand] створює команду 'LoadDataCommand',
        // яку ми можемо викликати з XAML або C#
        [RelayCommand]
        private async Task LoadDataAsync()
        {
            if (IsLoading) return; // Не завантажувати, якщо вже завантажується

            IsLoading = true;

            // Викликаємо наш API, передаючи обрані фільтри
            var listings = await _apiService.GetListingsAsync(SelectedFuel, SelectedVehicleType?.Id, SelectedMake?.Id, SelectedModel?.Id, SelectedCondition);

            Cars.Clear();
            foreach (var car in listings)
            {
                Cars.Add(car);
            }

            IsLoading = false;
        }

        // [RelayCommand] створює команду 'OpenFuelFilterCommand'
        [RelayCommand]
        private async Task OpenFuelFilterAsync()
        {
            // 1. Отримуємо дані з API (з нашої заглушки)
            var fuelTypes = await _apiService.GetFuelTypesAsync();

            // 2. Створюємо pop-up і передаємо йому дані
            var popup = new FuelFilterPopup(fuelTypes);

            // 3. Відкриваємо і чекаємо на результат
            var result = await Shell.Current.ShowPopupAsync(popup);

            // 4. Обробляємо результат
            if (result is string selectedValue)
            {
                // Змінюємо нашу властивість
                SelectedFuel = selectedValue;

                // Запускаємо фільтрацію (викликаємо 'LoadDataCommand')
                await LoadDataAsync();
            }
            // Якщо result == null (натиснули "X"), нічого не робимо
        }
        [RelayCommand]
        private async Task SelectConditionAsync(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return;

            // 1. Оновлюємо стан (це автоматично оновить стилі кнопок)
            SelectedCondition = condition;

            // 2. Перезавантажуємо дані з новим фільтром
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task OpenVehicleTypeFilterAsync()
        {
            // 1. Отримуємо реальні дані з API
            var types = await _apiService.GetVehicleTypesAsync();

            // 2. Створюємо Pop-up і передаємо йому дані
            var popup = new Popups.VehicleTypeFilterPopup(types);

            // 3. Відкриваємо і чекаємо на результат
            var result = await Shell.Current.ShowPopupAsync(popup);

            // 4. Обробляємо результат
            if (result is VehicleType selectedType)
            {
                // Користувач обрав тип
                SelectedVehicleType = selectedType;
            }
            else if (result is string clear && clear == "CLEAR")
            {
                // Користувач натиснув "Скинути фільтр"
                SelectedVehicleType = null;
            }
            // (Якщо result == null, користувач натиснув "X", нічого не робимо)

            // 5. Перезавантажуємо список оголошень
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task OpenMakeFilterAsync()
        {
            // TODO: Ми створимо цей Pop-up наступним
            await Shell.Current.DisplayAlert("Заглушка", "Pop-up 'Марка' буде тут", "OK");
        }

        [RelayCommand]
        private async Task OpenModelFilterAsync()
        {
            // Перевірка: не даємо обрати модель, поки не обрана марка
            if (SelectedMake == null)
            {
                await Shell.Current.DisplayAlert("Увага", "Спочатку оберіть Марку", "OK");
                return;
            }

            // Заглушка, яка показує, для якої марки ми шукаємо моделі
            await Shell.Current.DisplayAlert("Заглушка", $"Pop-up 'Модель' для {SelectedMake.Name} буде тут", "OK");
        }

        [RelayCommand]
        private async Task OpenYearFilterAsync()
        {
            // TODO: Ми створимо цей Pop-up наступним
            await Shell.Current.DisplayAlert("Заглушка", "Pop-up 'Рік' буде тут", "OK");
        }

        [RelayCommand]
        private async Task OpenPriceFilterAsync()
        {
            // TODO: Ми створимо цей Pop-up наступним
            await Shell.Current.DisplayAlert("Заглушка", "Pop-up 'Ціна' буде тут", "OK");
        }

        [RelayCommand]
        private async Task OpenRegionFilterAsync()
        {
            // TODO: Ми створимо цей Pop-up наступним
            await Shell.Current.DisplayAlert("Заглушка", "Pop-up 'Регіон' буде тут", "OK");
        }

        [RelayCommand]
        private async Task OpenTransmissionFilterAsync()
        {
            // TODO: Ми створимо цей Pop-up наступним
            await Shell.Current.DisplayAlert("Заглушка", "Pop-up 'Коробка передач' буде тут", "OK");
        }

        [RelayCommand]
        private async Task SwitchToUk()
        {
            // 1. Зберігаємо вибір у пам'ять телефону
            Preferences.Default.Set("AppLanguage", "uk-UA");

            // 2. Попереджаємо користувача, що треба перезапустити додаток
            await Shell.Current.DisplayAlert("Зміна мови", "Будь ласка, перезапустіть додаток, щоб застосувати українську мову.", "OK");
        }

        [RelayCommand]
        private async Task SwitchToEn()
        {
            // 1. Зберігаємо вибір у пам'ять телефону
            Preferences.Default.Set("AppLanguage", "en-US");

            // 2. Попереджаємо користувача, що треба перезапустити додаток
            await Shell.Current.DisplayAlert("Language Change", "Please restart the application to apply English language.", "OK");
        }

        private void OnLanguageChanged()
        {
            // Ми "змушуємо" UI оновити всі тексти,
            // які залежать від LocalizationManager
            OnPropertyChanged(nameof(FuelFilterText));
            OnPropertyChanged(nameof(VehicleTypeFilterText));
            OnPropertyChanged(nameof(MakeFilterText));
            OnPropertyChanged(nameof(ModelFilterText));
            OnPropertyChanged(nameof(TransmissionFilterText));
            // (додайте сюди інші, коли вони з'являться, напр. YearFilterText)
        }

    }
}