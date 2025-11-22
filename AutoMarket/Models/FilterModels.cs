using System.Text.Json.Serialization;

namespace AutoMarket.Models
{
    // Базовий клас, бо майже всі довідники мають ID та Name
    public class BaseDto
    {

        [JsonPropertyName("id")]   // <--- Кажемо, що в JSON це поле "id"
        public int Id { get; set; }

        [JsonPropertyName("name")] // <--- Кажемо, що в JSON це поле "name"
        public string Name { get; set; }
    }

    public class VehicleTypeDto : BaseDto { }

    public class VehicleBrandDto : BaseDto
    {
        public int VehicleTypeId { get; set; } // Щоб ми знали, до якого типу належить
    }

    public class VehicleModelDto : BaseDto
    {
        public int BrandId { get; set; } // Модель прив'язана до марки
    }

    public class RegionDto : BaseDto { }

    public class CityDto : BaseDto
    {
        public int RegionId { get; set; } // Місто прив'язане до регіону
    }

    public class FuelTypeDto : BaseDto { }

    public class GearTypeDto : BaseDto { }

    // Для перемикача Всі/Вживані/Нові
    public class VehicleConditionDto : BaseDto { }
}