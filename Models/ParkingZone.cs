namespace SmartParking.Models
{
    // Парковочная зона
    public class ParkingZone
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";        // Название зоны
        public string Address { get; set; } = "";     // Адрес
        public string ZoneType { get; set; } = "";    // Тип: платная, бесплатная, подземная и т.д.
        public int TotalSpots { get; set; }           // Всего мест
        public decimal PricePerHour { get; set; }     // Цена за час
        public string WorkingHours { get; set; } = ""; // Режим работы, например "08:00-22:00"
        public bool IsActive { get; set; } = true;

        // Навигационное свойство
        public List<ParkingSpot> Spots { get; set; } = new();
    }
}
