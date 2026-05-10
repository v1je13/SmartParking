namespace SmartParking.Models
{
    // Статус сессии парковки
    public enum SessionStatus
    {
        Active,     // Машина сейчас стоит
        Completed,  // Выехала и оплачено
        Unpaid,     // Выехала но не оплачено (долг)
        Fined,      // Выписан штраф
        Evacuated   // Отправлена на эвакуатор
    }

    // Сессия парковки (один заезд машины)
    public class ParkingSession
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }            // Какая машина
        public int ParkingSpotId { get; set; }        // На каком месте
        public DateTime EntryTime { get; set; }       // Время заезда
        public DateTime? ExitTime { get; set; }       // Время выезда (null если ещё стоит)
        public decimal? AmountDue { get; set; }       // Сколько нужно заплатить
        public decimal? AmountPaid { get; set; }      // Сколько заплатили
        public bool IsPaid { get; set; } = false;     // Оплачено или нет
        public decimal FineAmount { get; set; } = 0;  // Размер штрафа
        public string? FineReason { get; set; }       // Причина штрафа
        public SessionStatus Status { get; set; } = SessionStatus.Active;
        public string? Notes { get; set; }            // Примечания оператора
        
        // Абонемент
        public bool IsSubscription { get; set; } = false;
        public DateTime? SubscriptionExpiry { get; set; } // До когда действует абонемент

        // Навигационные свойства
        public Vehicle Vehicle { get; set; } = null!;
        public ParkingSpot Spot { get; set; } = null!;

        // Вычисляем сколько часов стояла машина
        public double GetHours()
        {
            var end = ExitTime ?? DateTime.Now;
            return (end - EntryTime).TotalHours;
        }
    }
}
