namespace AutoMarket.Models
{
    // Для GET /api/VehicleType
    public class VehicleType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // Для GET /api/VehicleCondition
    public class VehicleCondition
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ймовірно "Нові", "Вживані"
    }

    // Для GET /api/Region
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}