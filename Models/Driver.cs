namespace SmartParking.Models
{
    // Водитель
    public class Driver
    {
        public int Id { get; set; }
        public string LastName { get; set; } = "";    // Фамилия
        public string FirstName { get; set; } = "";   // Имя
        public string MiddleName { get; set; } = "";  // Отчество
        public string Phone { get; set; } = "";       // Телефон
        public string? Email { get; set; }            // Почта (необязательно)
        public string? DriverLicense { get; set; }    // Номер водительского
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public bool HasDebt { get; set; } = false;    // Есть ли долг

        // Полное ФИО - удобное свойство
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        // Навигационное свойство
        public List<Vehicle> Vehicles { get; set; } = new();
    }
}
