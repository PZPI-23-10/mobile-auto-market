using System.ComponentModel;

namespace AutoMarket.Models
{
    public class CarListing : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public int PriceUSD { get; set; }
        public int PriceUAH { get; set; }
        public double Mileage { get; set; }
        public string FuelType { get; set; }
        public string Location { get; set; }
        public string Transmission { get; set; }
        public string PostedDate { get; set; }

        public int VehicleTypeId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}