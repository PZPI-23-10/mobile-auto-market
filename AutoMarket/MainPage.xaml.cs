using System.Collections.ObjectModel;
using System.ComponentModel; // Потрібно для INotifyPropertyChanged
using CommunityToolkit.Maui.Views;
using AutoMarket.ViewModels; // Наш "мозок"
using AutoMarket.Models;

namespace AutoMarket
{
    


    public partial class MainPage : ContentPage
    {


        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

        }

  

    }
}