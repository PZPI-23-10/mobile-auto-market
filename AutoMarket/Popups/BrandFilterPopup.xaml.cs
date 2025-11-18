using CommunityToolkit.Maui.Views;
using System.Linq;

namespace AutoMarket
{
    public partial class BrandFilterPopup : Popup
    {
        public BrandFilterPopup()
        {
            InitializeComponent();

            // Викликаємо наш новий метод, щоб встановити розмір
            SetPopupSize();
        }

        private void SetPopupSize()
        {
            // 1. Отримуємо інформацію про екран (розміри, щільність пікселів)
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            // 2. Обчислюємо "справжню" висоту екрану в незалежних одиницях.
            //    (ділимо пікселі на щільність)
            double screenHeight = displayInfo.Height / displayInfo.Density;

            // 3. Встановлюємо висоту нашого Border (який ми назвали MainBorder в XAML)
            //    на 75% від висоти екрану.
            MainBorder.HeightRequest = screenHeight * 0.79;
            
        }

        // Обробник для кнопки закриття
        private void OnCloseButtonClicked(object sender, EventArgs e)
        {
            // Закриваємо pop-up
            Close();
        }

        private void OnBrandSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            // Отримуємо новий текст з поля пошуку,
            // робимо його нечутливим до регістру (нижні літери) та прибираємо пробіли
            var searchText = e.NewTextValue?.ToLowerInvariant().Trim();

            // Отримуємо всі Label з нашого списку, ОКРІМ заголовка "ТОП марки"
            var brandLabels = BrandListLayout.Children.OfType<Label>()
                                               .Where(lbl => lbl.Text != "ТОП марки");

            // Проходимо по кожному Label (Audi, BMW, і т.д.)
            foreach (var label in brandLabels)
            {
                // Якщо поле пошуку порожнє, показуємо всі бренди
                if (string.IsNullOrEmpty(searchText))
                {
                    label.IsVisible = true;
                    continue;
                }

                // Отримуємо назву бренду з Label
                var brandName = label.Text?.ToLowerInvariant();

                // МАГІЯ:
                // Робимо Label видимим (true) ТІЛЬКИ ЯКЩО
                // назва бренду (brandName) містить (Contains) текст пошуку (searchText)
                label.IsVisible = !string.IsNullOrEmpty(brandName) && brandName.Contains(searchText);
            }
        }

    }
}