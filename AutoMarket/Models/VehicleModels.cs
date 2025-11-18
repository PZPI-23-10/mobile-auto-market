namespace AutoMarket.Models
{
    //
    // Цей клас потрібен для ApiService.cs та MainPageViewModel.cs
    //
    public class Make
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    //
    // Цей клас також потрібен для ApiService.cs та MainPageViewModel.cs
    //
    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MakeId { get; set; }
    }
}