using AutoMarket.ViewModel;

namespace AutoMarket
{
    public partial class AddListingPage : ContentPage
    {
        public AddListingPage(AddListingViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}