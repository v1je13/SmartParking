using Avalonia.Controls;
using SmartParking.Data;
using SmartParking.Models;
using System;
using System.Linq;

namespace SmartParking.Views.Pages
{
    public partial class ZonesPage : UserControl
    {
        private AppDbContext _db;
        private ParkingZone? _editingZone;

        public ZonesPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            LoadZones();
            ClearForm();
        }

        private void LoadZones()
        {
            var zones = _db.ParkingZones.ToList();
            GridZones.ItemsSource = zones;
        }

        private void BtnAddZone_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _editingZone = null;
            ClearForm();
        }

        private void BtnEditZone_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridZones.SelectedItem is ParkingZone zone)
            {
                _editingZone = zone;
                TxtZoneName.Text = zone.Name;
                TxtZoneAddress.Text = zone.Address;
                CmbZoneType.SelectedItem = zone.ZoneType;
                TxtTotalSpots.Value = zone.TotalSpots;
                TxtPricePerHour.Value = (decimal)zone.PricePerHour;
                TxtWorkingHours.Text = zone.WorkingHours;
                ChkIsActive.IsChecked = zone.IsActive;
            }
        }

        private void BtnSaveZone_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Проверяем заполнено ли
            if (string.IsNullOrWhiteSpace(TxtZoneName.Text))
            {
                MessageBox.Show("Укажите название зоны!");
                return;
            }

            if (_editingZone == null)
            {
                // Добавляем новую
                _editingZone = new ParkingZone();
                _db.ParkingZones.Add(_editingZone);
            }

            // Заполняем свойства
            _editingZone.Name = TxtZoneName.Text;
            _editingZone.Address = TxtZoneAddress.Text;
            _editingZone.ZoneType = CmbZoneType.SelectedItem?.ToString() ?? "Платная";
            _editingZone.TotalSpots = (int)(TxtTotalSpots.Value ?? 0);
            _editingZone.PricePerHour = (decimal)(TxtPricePerHour.Value ?? 0);
            _editingZone.WorkingHours = TxtWorkingHours.Text;
            _editingZone.IsActive = ChkIsActive.IsChecked ?? false;

            _db.SaveChanges();
            LoadZones();
            ClearForm();
        }

        private void BtnDeleteZone_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridZones.SelectedItem is ParkingZone zone)
            {
                try
                {
                    _db.ParkingZones.Remove(zone);
                    _db.SaveChanges();
                    LoadZones();
                    MessageBox.Show("Зона удалена!");
                }
                catch
                {
                    MessageBox.Show("Не удалось удалить зону (возможно, есть места в этой зоне)!");
                }
            }
        }

        private void BtnCancelEdit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _editingZone = null;
            ClearForm();
        }

        private void BtnRefresh_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LoadZones();
        }

        private void ClearForm()
        {
            TxtZoneName.Clear();
            TxtZoneAddress.Clear();
            CmbZoneType.SelectedIndex = 0;
            TxtTotalSpots.Value = 0;
            TxtPricePerHour.Value = 0;
            TxtWorkingHours.Clear();
            ChkIsActive.IsChecked = true;
            GridZones.SelectedItem = null;
        }
    }

    // Простой MessageBox
    public static class MessageBox
    {
        public static async void Show(string message)
        {
            var dialog = new Window
            {
                Content = new StackPanel
                {
                    Padding = new Avalonia.Thickness(16),
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock { Text = message, Foreground = Avalonia.Media.Brushes.White },
                        new Button
                        {
                            Content = "OK",
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                            Width = 100
                        }
                    }
                },
                Title = "Сообщение",
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            if (dialog.FindControl<Button>("") is Button btn)
            {
                btn.Click += (s, e) => dialog.Close();
            }
            else
            {
                // Простой вариант - просто выводим в консоль
                System.Console.WriteLine("✓ " + message);
            }
        }
    }
}
