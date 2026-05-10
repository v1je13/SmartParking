using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SmartParking.Data;
using SmartParking.Models;
using System;
using System.Linq;

namespace SmartParking.Views.Pages
{
    public partial class SessionsPage : UserControl
    {
        private AppDbContext _db;
        private ParkingSession? _editingSession;
        private string _filterStatus = "Все";

        public SessionsPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            LoadVehiclesForCombo();
            LoadSpotsForCombo();
            LoadSessions();
            ClearForm();
            CmbFilterStatus.SelectedIndex = 0;
        }

        private void LoadVehiclesForCombo()
        {
            var vehicles = _db.Vehicles.Include(v => v.Driver).ToList();
            var items = vehicles.Select(v => new { v.Id, Display = v.LicensePlate }).ToList();
            CmbVehicle.ItemsSource = items;
        }

        private void LoadSpotsForCombo()
        {
            var spots = _db.ParkingSpots.Include(s => s.Zone).ToList();
            var items = spots.Select(s => new { s.Id, Display = $"{s.Zone.Name} / {s.SpotNumber}" }).ToList();
            CmbSpot.ItemsSource = items;
        }

        private void LoadSessions()
        {
            var sessions = _db.ParkingSessions
                .Include(s => s.Vehicle)
                .Include(s => s.Vehicle.Driver)
                .Include(s => s.Spot)
                .Include(s => s.Spot.Zone)
                .ToList();

            // Фильтруем по статусу
            if (_filterStatus != "Все" && Enum.TryParse<SessionStatus>(_filterStatus, out var status))
            {
                sessions = sessions.Where(s => s.Status == status).ToList();
            }

            GridSessions.ItemsSource = sessions;
        }

        private void CmbFilterStatus_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            _filterStatus = CmbFilterStatus.SelectedItem?.ToString() ?? "Все";
            LoadSessions();
        }

        private void BtnNewSession_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _editingSession = null;
            ClearForm();
        }

        private void BtnEditSession_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSessions.SelectedItem is ParkingSession session)
            {
                _editingSession = session;
                
                // Выбираем машину
                var vehicleItem = CmbVehicle.ItemsSource?.Cast<dynamic>()
                    .FirstOrDefault(v => v.Id == session.VehicleId);
                if (vehicleItem != null)
                    CmbVehicle.SelectedItem = vehicleItem;

                // Выбираем место
                var spotItem = CmbSpot.ItemsSource?.Cast<dynamic>()
                    .FirstOrDefault(s => s.Id == session.ParkingSpotId);
                if (spotItem != null)
                    CmbSpot.SelectedItem = spotItem;

                TxtEntryTime.Text = session.EntryTime.ToString("dd.MM.yyyy HH:mm");
                TxtExitTime.Text = session.ExitTime?.ToString("dd.MM.yyyy HH:mm") ?? "";
                TxtAmountDue.Value = (decimal?)session.AmountDue ?? 0;
                CmbSessionStatus.SelectedItem = session.Status.ToString();
            }
        }

        private void BtnSaveSession_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (CmbVehicle.SelectedItem == null || CmbSpot.SelectedItem == null)
            {
                System.Console.WriteLine("Выберите машину и место!");
                return;
            }

            if (_editingSession == null)
            {
                _editingSession = new ParkingSession();
                _db.ParkingSessions.Add(_editingSession);
            }

            // Получаем ID машины и места
            var selectedVehicle = CmbVehicle.SelectedItem as dynamic;
            var selectedSpot = CmbSpot.SelectedItem as dynamic;

            _editingSession.VehicleId = selectedVehicle.Id;
            _editingSession.ParkingSpotId = selectedSpot.Id;
            
            if (DateTime.TryParse(TxtEntryTime.Text, out var entryTime))
                _editingSession.EntryTime = entryTime;

            if (!string.IsNullOrEmpty(TxtExitTime.Text) && DateTime.TryParse(TxtExitTime.Text, out var exitTime))
                _editingSession.ExitTime = exitTime;

            _editingSession.AmountDue = (decimal?)TxtAmountDue.Value;

            var statusStr = CmbSessionStatus.SelectedItem?.ToString() ?? "Active";
            if (Enum.TryParse<SessionStatus>(statusStr, out var status))
                _editingSession.Status = status;

            _db.SaveChanges();
            LoadSessions();
            ClearForm();
            System.Console.WriteLine("✓ Сессия сохранена!");
        }

        private void BtnMarkExit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSessions.SelectedItem is ParkingSession session)
            {
                if (session.Status == SessionStatus.Active)
                {
                    session.ExitTime = DateTime.Now;
                    // Вычисляем сумму к оплате
                    var zone = session.Spot.Zone;
                    if (zone != null)
                    {
                        var hours = Math.Ceiling(session.GetHours());
                        session.AmountDue = (decimal)hours * zone.PricePerHour;
                    }
                    session.Status = SessionStatus.Completed;
                    _db.SaveChanges();
                    LoadSessions();
                    System.Console.WriteLine("✓ Отмечен выезд!");
                }
            }
        }

        private void BtnMarkPaid_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSessions.SelectedItem is ParkingSession session)
            {
                session.AmountPaid = session.AmountDue;
                session.IsPaid = true;
                session.Status = SessionStatus.Completed;
                _db.SaveChanges();
                LoadSessions();
                System.Console.WriteLine("✓ Отмечено как оплачено!");
            }
        }

        private void BtnAddFine_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSessions.SelectedItem is ParkingSession session)
            {
                session.FineAmount = 500; // Штраф 500 руб (по умолчанию)
                session.FineReason = "Нарушение парковки";
                session.Status = SessionStatus.Fined;
                _db.SaveChanges();
                LoadSessions();
                System.Console.WriteLine("✓ Выписан штраф!");
            }
        }

        private void BtnEvacuate_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSessions.SelectedItem is ParkingSession session)
            {
                session.Status = SessionStatus.Evacuated;
                session.ExitTime = DateTime.Now;
                _db.SaveChanges();
                LoadSessions();
                System.Console.WriteLine("✓ Машина отмечена как эвакуированная!");
            }
        }

        private void BtnCancelEdit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _editingSession = null;
            ClearForm();
        }

        private void BtnRefresh_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LoadSessions();
        }

        private void ClearForm()
        {
            CmbVehicle.SelectedIndex = -1;
            CmbSpot.SelectedIndex = -1;
            TxtEntryTime.Clear();
            TxtExitTime.Clear();
            TxtAmountDue.Value = 0;
            CmbSessionStatus.SelectedIndex = 0;
            GridSessions.SelectedItem = null;
        }
    }
}
