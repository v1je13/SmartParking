namespace SmartParking.Models
{
    using System.Collections.Generic;

    // Машина
    public class Vehicle
    {
        public int Id { get; set; }
        public int DriverId { get; set; }             // Внешний ключ на водителя
        public string LicensePlate { get; set; } = ""; // Гос. номер, например А123БВ77
        public string Brand { get; set; } = "";        // Марка (Toyota, BMW...)
        public string Model { get; set; } = "";        // Модель (Camry, X5...)
        public string Color { get; set; } = "";        // Цвет
        public int Year { get; set; }                  // Год выпуска
        public string VehicleType { get; set; } = "Легковой"; // Тип: легковой, грузовой, мотоцикл

        // Навигационные свойства
        public Driver Driver { get; set; } = null!;
        public List<ParkingSession> Sessions { get; set; } = new();
    }
}
