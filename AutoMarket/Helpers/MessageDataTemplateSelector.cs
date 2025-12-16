using AutoMarket.Models;

namespace AutoMarket.Helpers // Зверни увагу на namespace
{
    public class MessageDataTemplateSelector : DataTemplateSelector
    {
        // Сюди ми в XAML покладемо шаблон для "Моїх"
        public DataTemplate MyMessageTemplate { get; set; }

        // Сюди покладемо шаблон для "Чужих"
        public DataTemplate OtherMessageTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            // Отримуємо повідомлення
            var message = (ChatMessageDto)item;

            // Якщо воно моє - повертаємо шаблон MyMessageTemplate, інакше - OtherMessageTemplate
            return message.IsMine ? MyMessageTemplate : OtherMessageTemplate;
        }
    }
}