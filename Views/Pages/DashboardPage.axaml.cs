using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SmartParking.Data;
using SmartParking.Models;
using System;
using System.Linq;

namespace SmartParking.Views.Pages
{
    public partial class DashboardPage : UserControl
    {
        private AppDbContext _db;

        public DashboardPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            LoadData();
        }

        private void LoadData()
        {
            // Общая статистика по местам
            var allSpots = _db.ParkingSpots.ToList();
            var freeSpots = allSpots.Count(s => s.Status == SpotStatus.Free);
            var occupiedSpots = allSpots.Count(s => s.Status == SpotStatus.Occupied);
            var repairSpots = allSpots.Count(s => s.Status == SpotStatus.Repair);

            TxtTotalSpots.Text = allSpots.Count.ToString();
            TxtFreeSpots.Text = freeSpots.ToString();
            TxtOccupiedSpots.Text = occupiedSpots.ToString();
            TxtRepairSpots.Text = repairSpots.ToString();

            // Водители
            var driversCount = _db.Drivers.Count();
            TxtDriversCount.Text = driversCount.ToString();

            // Должники (неоплаченные сессии)
            var debtors = _db.ParkingSessions.Count(s => s.Status == SessionStatus.Unpaid);
            TxtDebtors.Text = debtors.ToString();

            // Активные сессии
            var activeSessions = _db.ParkingSessions
                .Where(s => s.Status == SessionStatus.Active)
                .Include(s => s.Vehicle)
                .Include(s => s.Vehicle.Driver)
                .Include(s => s.Spot)
                .Include(s => s.Spot.Zone)
                .ToList();

            // Преобразуем для отображения
            var displaySessions = activeSessions.Select(s => new
            {
                s.Id,
                s.Vehicle,
                VehicleInfo = $"{s.Vehicle?.Brand} {s.Vehicle?.Model}",
                OwnerName = s.Vehicle?.Driver?.FullName ?? "—",
                ZoneName = s.Spot?.Zone?.Name ?? "—",
                s.Spot,
                s.EntryTime,
                HoursParked = Math.Round(s.GetHours(), 1)
            }).ToList();

            GridActiveSessions.ItemsSource = displaySessions;
        }
    }
}
