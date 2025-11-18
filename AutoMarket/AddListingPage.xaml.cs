using System.Collections.ObjectModel; // Потрібен для списку

namespace AutoMarket
{
    public partial class AddListingPage : ContentPage
    {
        // Список для зберігання шляхів до обраних фото
        private ObservableCollection<string> photoPaths = new ObservableCollection<string>();

        public AddListingPage()
        {
            InitializeComponent();
        }

        // ==========================================================
        //     НОВИЙ МЕТОД: ОБРОБНИК НАТИСКАННЯ КНОПКИ "ДОДАТИ ФОТО"
        // ==========================================================
        private async void OnPickImageClicked(object sender, EventArgs e)
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                // Запитуємо у користувача, звідки брати фото
                string action = await DisplayActionSheet("Додати фото", "Скасувати", null, "Обрати з галереї", "Зробити знімок");

                FileResult photo = null;

                if (action == "Обрати з галереї")
                {
                    // Відкриваємо галерею
                    photo = await MediaPicker.Default.PickPhotoAsync();
                }
                else if (action == "Зробити знімок")
                {
                    // Відкриваємо камеру
                    photo = await MediaPicker.Default.CapturePhotoAsync();
                }

                // Якщо користувач обрав фото (а не натиснув "Скасувати")
                if (photo != null)
                {
                    // Зберігаємо шлях до файлу
                    string localFilePath = photo.FullPath;
                    photoPaths.Add(localFilePath);

                    // Оновлюємо наш список мініатюр
                    UpdateImagePreviews();
                }
            }
            else
            {
                // Якщо пристрій не підтримує вибір медіа (наприклад, емулятор без камери)
                // Просто відкриємо галерею
                try
                {
                    var photo = await MediaPicker.Default.PickPhotoAsync();
                    if (photo != null)
                    {
                        string localFilePath = photo.FullPath;
                        photoPaths.Add(localFilePath);
                        UpdateImagePreviews();
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Помилка", $"Не вдалося відкрити галерею: {ex.Message}", "OK");
                }
            }
        }

        // ==========================================================
        //     НОВИЙ МЕТОД: ОНОВЛЕННЯ СПИСКУ МІНІАТЮР
        // ==========================================================
        private void UpdateImagePreviews()
        {
            // Очищуємо старий список мініатюр
            ImagePreviewLayout.Clear();

            // Створюємо мініатюри для кожного фото у нашому списку photoPaths
            foreach (string path in photoPaths)
            {
                var image = new Image
                {
                    Source = ImageSource.FromFile(path),
                    WidthRequest = 100,
                    HeightRequest = 100,
                    Aspect = Aspect.AspectFill
                };

                // Додаємо мініатюру в наш HorizontalStackLayout
                ImagePreviewLayout.Children.Add(image);
            }
        }
    }
}