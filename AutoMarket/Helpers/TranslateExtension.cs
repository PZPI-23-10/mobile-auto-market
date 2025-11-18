using AutoMarket.Services;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;

namespace AutoMarket.Helpers
{
    // Цей клас дозволяє нам писати {helpers:Translate KeyName}
    [ContentProperty(nameof(Key))]
    public class TranslateExtension : IMarkupExtension<BindingBase>
    {
        public string Key { get; set; }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            // Ми створюємо прив'язку до нашого "мозку" (LocalizationManager.Instance)
            // і до його властивості "this[Key]" (індексатора)
            return new Binding
            {
                Path = $"[{Key}]",
                Source = LocalizationManager.Instance
            };
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }
    }
}