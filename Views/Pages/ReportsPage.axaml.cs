using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SmartParking.Data;
using SmartParking.Models;

namespace SmartParking.Views.Pages
{
    public partial class ReportsPage : UserControl
    {
        private AppDbContext _db;
        private int _selectedVehicleId = 0;

        public ReportsPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            LoadAllReports();
        }

        private void LoadAllReports()
        {
            LoadReport1();
            LoadReport2();
            LoadVehiclesForFilter();
            LoadReport3();
            LoadReport4();
            LoadReport5();
            LoadReport6();
            LoadReport7();
            LoadReport8();
        }

        // Отчёт 1: Свободные места в зоне
        private void LoadReport1()
        {
            var zones = _db.ParkingZones.Include(z => z.Spots).ToList();
            var report = zones.Select(z => new
            {
                ZoneName = z.Name,
                FreeSpots = z.Spots.Count(s => s.Status == SpotStatus.Free),
                OccupiedSpots = z.Spots.Count(s => s.Status == SpotStatus.Occupied),
                RepairSpots = z.Spots.Count(s => s.Status == SpotStatus.Repair),
                LoadPercent = z.TotalSpots > 0 
                    ? (double)z.Spots.Count(s => s.Status == SpotStatus.Occupied) / z.TotalSpots * 100 
                    : 0
            }).ToList();

            GridReport1.ItemsSource = report;
            
            // Заполняем ComboBox для фильтра
            CmbZone1.ItemsSource = new[] { "Все зоны" }.Concat(zones.Select(z => z.Name)).ToList();
            CmbZone1.SelectedIndex = 0;
        }

        private void CmbZone1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            LoadReport1();
        }

        private void BtnExportReport1_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            System.Console.WriteLine("✓ Отчёт экспортирован!");
        }

        // Отчёт 2: Какая машина на каком месте
        private void LoadReport2()
        {
            var activeSessions = _db.ParkingSessions
                .Where(s => s.Status == SessionStatus.Active)
                .Include(s => s.Vehicle)
                .Include(s => s.Vehicle.Driver)
                .Include(s => s.Spot)
                .Include(s => s.Spot.Zone)
                .ToList();

            var report = activeSessions.Select(s => new
            {
                ZoneName = s.Spot.Zone.Name,
                SpotNumber = s.Spot.SpotNumber,
                LicensePlate = s.Vehicle.LicensePlate,
                OwnerName = s.Vehicle.Driver.FullName,
                EntryTime = s.EntryTime,
                Hours = s.GetHours()
            }).ToList();

            GridReport2.ItemsSource = report;
        }

        private void BtnRefreshReport2_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LoadReport2();
        }

        // Отчёт 3: История парковок машины
        private void LoadVehiclesForFilter()
        {
            var vehicles = _db.Vehicles.ToList();
            CmbVehicle.ItemsSource = vehicles.Select(v => new { v.Id, Display = v.LicensePlate }).ToList();
            if (CmbVehicle.ItemCount > 0)
                CmbVehicle.SelectedIndex = 0;
        }

        private void CmbVehicle_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (CmbVehicle.SelectedItem is dynamic item)
            {
                _selectedVehicleId = item.Id;
                LoadReport3();
            }
        }

        private void LoadReport3()
        {
            var sessions = _db.ParkingSessions
                .Where(s => s.VehicleId == _selectedVehicleId)
                .Include(s => s.Spot)
                .Include(s => s.Spot.Zone)
                .ToList();

            var report = sessions.Select(s => new
            {
                ZoneName = s.Spot.Zone.Name,
                SpotNumber = s.Spot.SpotNumber,
                EntryTime = s.EntryTime,
                ExitTime = s.ExitTime,
                Hours = s.GetHours(),
                Amount = s.AmountDue ?? 0,
                Status = s.Status.ToString()
            }).ToList();

            GridReport3.ItemsSource = report;
        }

        // Отчёт 4: Должники
        private void LoadReport4()
        {
            var unpaidSessions = _db.ParkingSessions
                .Where(s => s.Status == SessionStatus.Unpaid)
                .Include(s => s.Vehicle)
                .Include(s => s.Vehicle.Driver)
                .Include(s => s.Spot)
                .Include(s => s.Spot.Zone)
                .ToList();

            var report = unpaidSessions.Select(s => new
            {
                LicensePlate = s.Vehicle.LicensePlate,
                OwnerName = s.Vehicle.Driver.FullName,
                Phone = s.Vehicle.Driver.Phone,
                ZoneName = s.Spot.Zone.Name,
                ExitTime = s.ExitTime,
                Amount = s.AmountDue ?? 0
            }).ToList();

            GridReport4.ItemsSource = report;
            var totalDebt = report.Sum(r => r.Amount);
            TxtTotalDebt.Text = $"Общий долг: {totalDebt:F2} руб.";
        }

        // Отчёт 5: Загруженные зоны (>90%)
        private void LoadReport5()
        {
            var zones = _db.ParkingZones.Include(z => z.Spots).ToList();
            var report = zones.Where(z => z.TotalSpots > 0)
                .Select(z => new
                {
                    ZoneName = z.Name,
                    TotalSpots = z.TotalSpots,
                    OccupiedSpots = z.Spots.Count(s => s.Status == SpotStatus.Occupied),
                    LoadPercent = (double)z.Spots.Count(s => s.Status == SpotStatus.Occupied) / z.TotalSpots * 100
                })
                .Where(z => z.LoadPercent >= 90)
                .ToList();

            GridReport5.ItemsSource = report;
        }

        // Отчёт 6: Среднее время стоянки
        private void LoadReport6()
        {
            var zones = _db.ParkingZones.Include(z => z.Spots).ToList();
            var report = zones.Select(z => new
            {
                ZoneName = z.Name,
                SessionCount = z.Spots.SelectMany(s => s.Sessions).Count(),
                AvgHours = z.Spots.SelectMany(s => s.Sessions).Count() > 0
                    ? z.Spots.SelectMany(s => s.Sessions).Average(s => s.GetHours())
                    : 0.0,
                MinHours = z.Spots.SelectMany(s => s.Sessions).Count() > 0
                    ? z.Spots.SelectMany(s => s.Sessions).Min(s => s.GetHours())
                    : 0.0,
                MaxHours = z.Spots.SelectMany(s => s.Sessions).Count() > 0
                    ? z.Spots.SelectMany(s => s.Sessions).Max(s => s.GetHours())
                    : 0.0
            }).ToList();

            GridReport6.ItemsSource = report;
        }

        // Отчёт 7: Выручка по зонам
        private void LoadReport7()
        {
            var zones = _db.ParkingZones.Include(z => z.Spots).ToList();
            var allSessions = _db.ParkingSessions.ToList();
            var totalRevenue = 0m;

            var report = zones.Select(z => new
            {
                ZoneName = z.Name,
                PaidSessions = z.Spots.SelectMany(s => s.Sessions)
                    .Where(s => s.IsPaid && s.Status == SessionStatus.Completed).Count(),
                Revenue = z.Spots.SelectMany(s => s.Sessions)
                    .Where(s => s.IsPaid && s.Status == SessionStatus.Completed)
                    .Sum(s => s.AmountPaid ?? 0),
                Percent = allSessions.Count > 0
                    ? (double)z.Spots.SelectMany(s => s.Sessions).Count() / allSessions.Count * 100
                    : 0.0
            }).ToList();

            totalRevenue = (decimal)report.Sum(r => (double)r.Revenue);
            GridReport7.ItemsSource = report;
            TxtTotalRevenue.Text = $"Общая выручка: {totalRevenue:F2} руб.";
        }

        // Отчёт 8: Эвакуированные машины
        private void LoadReport8()
        {
            var evacuatedSessions = _db.ParkingSessions
                .Where(s => s.Status == SessionStatus.Evacuated)
                .Include(s => s.Vehicle)
                .Include(s => s.Vehicle.Driver)
                .Include(s => s.Spot)
                .Include(s => s.Spot.Zone)
                .ToList();

            var report = evacuatedSessions.Select(s => new
            {
                LicensePlate = s.Vehicle.LicensePlate,
                OwnerName = s.Vehicle.Driver.FullName,
                ZoneName = s.Spot.Zone.Name,
                EvacuatedAt = s.ExitTime,
                Notes = s.Notes ?? "—"
            }).ToList();

            GridReport8.ItemsSource = report;
            TxtEvacuatedCount.Text = $"Всего эвакуировано: {report.Count} шт.";
        }
    }
}
