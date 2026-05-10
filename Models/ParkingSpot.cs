namespace SmartParking.Models
{
    // Статусы парковочного места
    public enum SpotStatus
    {
        Free,       // Свободно
        Occupied,   // Занято
        Repair      // На ремонте
    }

    // Парковочное место
    public class ParkingSpot
    {
        public int Id { get; set; }
        public int ParkingZoneId { get; set; }        // Внешний ключ на зону
        public string SpotNumber { get; set; } = "";  // Номер места (например А1, Б2)
        public string SpotType { get; set; } = "";    // Тип: обычное, для инвалидов, для грузовых
        public SpotStatus Status { get; set; } = SpotStatus.Free;
        public string? Notes { get; set; }            // Примечания

        // Навигационные свойства
        public ParkingZone Zone { get; set; } = null!;
        public List<ParkingSession> Sessions { get; set; } = new();
    }
}
