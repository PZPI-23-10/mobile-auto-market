using CommunityToolkit.Maui.Views;
using System.Linq; // Потрібен для .OfType<> та .FirstOrDefault()

namespace AutoMarket
{
    public partial class RegionFilterPopup : Popup
    {
        public RegionFilterPopup()
        {
            InitializeComponent();
            SetPopupSize();
        }

        // Встановлює розмір Pop-up на 100% екрану
        private void SetPopupSize()
        {
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            double screenHeight = displayInfo.Height / displayInfo.Density;
            double screenWidth = displayInfo.Width / displayInfo.Density;

            MainBorder.HeightRequest = screenHeight * 0.79;
            MainBorder.WidthRequest = screenWidth;
        }

        // Закриває Pop-up
        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            Close();
        }

        // ==========================================================
        //     НОВИЙ МЕТОД: ФІЛЬТРАЦІЯ РЕГІОНІВ
        // ==========================================================
        private void OnRegionSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.ToLowerInvariant().Trim();

            // Отримуємо всі елементи зі списку
            var children = RegionListLayout.Children;

            Label currentHeading = null; // Поточний заголовок (Північ, Центр...)
            VerticalStackLayout currentRegionGroup = null; // Поточна група (список областей)

            bool anyVisibleInCurrentGroup = false;

            foreach (var child in children)
            {
                // 1. Якщо це ЗАГОЛОВОК ("Північ", "Центр"...)
                if (child is Label heading)
                {
                    // Якщо ми вже маємо попередній заголовок,
                    // встановлюємо його видимість на основі того, чи були видимі елементи в його групі
                    if (currentHeading != null)
                    {
                        currentHeading.IsVisible = anyVisibleInCurrentGroup;
                        currentRegionGroup.IsVisible = anyVisibleInCurrentGroup;
                    }

                    // Починаємо нову групу
                    currentHeading = heading;
                    anyVisibleInCurrentGroup = false; // Скидаємо лічильник видимості
                }
                // 2. Якщо це ГРУПА РЕГІОНІВ (контейнер VerticalStackLayout)
                else if (child is VerticalStackLayout regionGroup)
                {
                    currentRegionGroup = regionGroup;
                    // Проходимо по кожному рядку (HorizontalStackLayout) всередині групи
                    foreach (var row in regionGroup.Children.OfType<HorizontalStackLayout>())
                    {
                        // Знаходимо Label в цьому рядку
                        var label = row.Children.OfType<Label>().FirstOrDefault();
                        if (label == null) continue;

                        var regionName = label.Text?.ToLowerInvariant();

                        // Якщо поле пошуку порожнє, показуємо все
                        if (string.IsNullOrEmpty(searchText))
                        {
                            row.IsVisible = true;
                            anyVisibleInCurrentGroup = true;
                        }
                        else
                        {
                            // Перевіряємо, чи містить назва регіону текст пошуку
                            bool isVisible = !string.IsNullOrEmpty(regionName) && regionName.Contains(searchText);
                            row.IsVisible = isVisible;

                            if (isVisible)
                            {
                                anyVisibleInCurrentGroup = true; // Позначаємо, що в цій групі є видимий елемент
                            }
                        }
                    }
                }
            }

            // Перевірка для ОСТАННЬОЇ групи в списку (оскільки цикл завершився)
            if (currentHeading != null)
            {
                currentHeading.IsVisible = anyVisibleInCurrentGroup;
                if (currentRegionGroup != null)
                {
                    currentRegionGroup.IsVisible = anyVisibleInCurrentGroup;
                }
            }
        }
    }
}