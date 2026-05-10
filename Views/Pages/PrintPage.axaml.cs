using Avalonia;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SmartParking.Data;
using SmartParking.Models;
using SmartParking.Services;

namespace SmartParking.Views.Pages
{
    public partial class PrintPage : UserControl
    {
        private AppDbContext _db;
        private WordPrinter _wordPrinter;
        private string _filterStatus = "Все";

        public PrintPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            _wordPrinter = new WordPrinter();
            LoadSessions();
            CmbFilterStatus.SelectedIndex = 0;
            ClearInfo();
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
            ClearInfo();
        }

        private void BtnPreview_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSessions.SelectedItem is ParkingSession session)
            {
                try
                {
                    _wordPrinter.PreviewReceipt(session);
                    System.Console.WriteLine("✓ Квитанция открыта в Word!");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"✗ Ошибка: {ex.Message}");
                }
            }
            else
            {
                System.Console.WriteLine("✗ Выберите сессию!");
            }
        }

        private void BtnPrint_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSessions.SelectedItem is ParkingSession session)
            {
                try
                {
                    // Отмечаем как оплачено если ещё не отмечено
                    if (!session.IsPaid && session.AmountDue.HasValue)
                    {
                        session.AmountPaid = session.AmountDue;
                        session.IsPaid = true;
                        _db.SaveChanges();
                    }

                    _wordPrinter.PrintReceipt(session);
                    System.Console.WriteLine("✓ Квитанция отправлена на печать!");
                    LoadSessions();
                    ClearInfo();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"✗ Ошибка при печати: {ex.Message}");
                }
            }
            else
            {
                System.Console.WriteLine("✗ Выберите сессию!");
            }
        }

        private void BtnRefresh_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LoadSessions();
        }

        // Обновляем информацию при выборе сессии
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            // При клике на строку таблицы обновляем информацию
            if (GridSessions.SelectedItem is ParkingSession session)
            {
                UpdateSessionInfo(session);
            }
        }

        private void UpdateSessionInfo(ParkingSession session)
        {
            TxtPlate.Text = session.Vehicle?.LicensePlate ?? "—";
            TxtOwner.Text = session.Vehicle?.Driver?.FullName ?? "—";
            TxtZone.Text = session.Spot?.Zone?.Name ?? "—";
            TxtSpot.Text = session.Spot?.SpotNumber ?? "—";
            TxtEntry.Text = session.EntryTime.ToString("dd.MM.yyyy HH:mm");
            TxtExit.Text = session.ExitTime?.ToString("dd.MM.yyyy HH:mm") ?? "ещё стоит";
            TxtHours.Text = session.GetHours().ToString("F1") + " ч.";
            TxtAmountDue.Text = (session.AmountDue ?? 0).ToString("F2") + " руб.";
            TxtFine.Text = session.FineAmount > 0 
                ? session.FineAmount.ToString("F2") + " руб." 
                : "—";
            
            decimal total = (session.AmountDue ?? 0) + session.FineAmount;
            TxtTotal.Text = total.ToString("F2") + " руб.";
            
            if (session.IsPaid)
                TxtStatus.Text = "✓ ОПЛАЧЕНО";
            else if (session.Status == SessionStatus.Unpaid)
                TxtStatus.Text = "НЕ ОПЛАЧЕНО";
            else if (session.Status == SessionStatus.Fined)
                TxtStatus.Text = "ШТРАФ";
            else if (session.Status == SessionStatus.Evacuated)
                TxtStatus.Text = "ЭВАКУИРОВАНО";
            else
                TxtStatus.Text = session.Status.ToString();
        }

        private void ClearInfo()
        {
            TxtPlate.Text = "—";
            TxtOwner.Text = "—";
            TxtZone.Text = "—";
            TxtSpot.Text = "—";
            TxtEntry.Text = "—";
            TxtExit.Text = "—";
            TxtHours.Text = "—";
            TxtAmountDue.Text = "—";
            TxtFine.Text = "—";
            TxtTotal.Text = "—";
            TxtStatus.Text = "—";
        }
    }
}
