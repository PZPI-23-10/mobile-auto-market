<<<<<<< Updated upstream
﻿namespace AutoMarket
=======
﻿using System.Collections.ObjectModel;
using System.ComponentModel; // Потрібно для INotifyPropertyChanged
using CommunityToolkit.Maui.Views;
using AutoMarket.ViewModel; // Наш "мозок"
using AutoMarket.Models;

namespace AutoMarket
>>>>>>> Stashed changes
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
