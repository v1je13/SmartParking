using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SmartParking.Data;
using SmartParking.Models;
using System;
using System.Linq;

namespace SmartParking.Views.Pages
{
    public partial class SpotsPage : UserControl
    {
        private AppDbContext _db;
        private ParkingSpot? _editingSpot;
        private int _filterZoneId = 0;

        public SpotsPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            LoadZonesForFilter();
            LoadSpots();
            ClearForm();
        }

        private void LoadZonesForFilter()
        {
            // Очищаем старые элементы кроме первого
            while (CmbFilterZone.ItemCount > 1)
            {
                CmbFilterZone.ItemsSource = null;
            }

            CmbFilterZone.ItemsSource = new[] { "Все зоны" }
                .Concat(_db.ParkingZones.Select(z => z.Name).ToList())
                .ToList();
            CmbFilterZone.SelectedIndex = 0;

            // Заполняем ComboBox для зоны при редактировании
            CmbZone.ItemsSource = _db.ParkingZones.Select(z => new { z.Id, z.Name }).ToList();
        }

        private void LoadSpots()
        {
            List<ParkingSpot> spots;

            if (_filterZoneId == 0)
            {
                spots = _db.ParkingSpots.Include(s => s.Zone).ToList();
            }
            else
            {
                spots = _db.ParkingSpots.Where(s => s.ParkingZoneId == _filterZoneId)
                    .Include(s => s.Zone).ToList();
            }

            GridSpots.ItemsSource = spots;
        }

        private void CmbFilterZone_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selected = CmbFilterZone.SelectedItem?.ToString();
            if (selected == "Все зоны")
            {
                _filterZoneId = 0;
            }
            else if (selected != null)
            {
                var zone = _db.ParkingZones.FirstOrDefault(z => z.Name == selected);
                _filterZoneId = zone?.Id ?? 0;
            }
            LoadSpots();
        }

        private void BtnAddSpot_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _editingSpot = null;
            ClearForm();
        }

        private void BtnEditSpot_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSpots.SelectedItem is ParkingSpot spot)
            {
                _editingSpot = spot;
                // Найти и выбрать зону
                var zoneItem = CmbZone.ItemsSource?.Cast<dynamic>()
                    .FirstOrDefault(z => z.Id == spot.ParkingZoneId);
                if (zoneItem != null)
                    CmbZone.SelectedItem = zoneItem;
                
                TxtSpotNumber.Text = spot.SpotNumber;
                CmbSpotType.SelectedItem = spot.SpotType;
                CmbStatus.SelectedItem = spot.Status.ToString();
            }
        }

        private void BtnSaveSpot_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSpotNumber.Text))
            {
                System.Console.WriteLine("Укажите номер места!");
                return;
            }

            if (CmbZone.SelectedItem == null)
            {
                System.Console.WriteLine("Выберите зону!");
                return;
            }

            if (_editingSpot == null)
            {
                _editingSpot = new ParkingSpot();
                _db.ParkingSpots.Add(_editingSpot);
            }

            // Получаем ID зоны
            var selectedZone = CmbZone.SelectedItem as dynamic;
            _editingSpot.ParkingZoneId = selectedZone.Id;
            _editingSpot.SpotNumber = TxtSpotNumber.Text;
            _editingSpot.SpotType = CmbSpotType.SelectedItem?.ToString() ?? "Обычное";
            
            // Парсим статус
            var statusStr = CmbStatus.SelectedItem?.ToString() ?? "Free";
            if (Enum.TryParse<SpotStatus>(statusStr, out var status))
            {
                _editingSpot.Status = status;
            }

            _db.SaveChanges();
            LoadSpots();
            ClearForm();
            System.Console.WriteLine("✓ Место сохранено!");
        }

        private void BtnDeleteSpot_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSpots.SelectedItem is ParkingSpot spot)
            {
                try
                {
                    _db.ParkingSpots.Remove(spot);
                    _db.SaveChanges();
                    LoadSpots();
                    System.Console.WriteLine("✓ Место удалено!");
                }
                catch
                {
                    System.Console.WriteLine("✗ Не удалось удалить место!");
                }
            }
        }

        private void BtnMarkFree_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSpots.SelectedItem is ParkingSpot spot)
            {
                spot.Status = SpotStatus.Free;
                _db.SaveChanges();
                LoadSpots();
                System.Console.WriteLine("✓ Место отмечено как свободное!");
            }
        }

        private void BtnMarkRepair_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridSpots.SelectedItem is ParkingSpot spot)
            {
                spot.Status = SpotStatus.Repair;
                _db.SaveChanges();
                LoadSpots();
                System.Console.WriteLine("✓ Место отмечено в ремонте!");
            }
        }

        private void BtnCancelEdit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _editingSpot = null;
            ClearForm();
        }

        private void BtnRefresh_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LoadSpots();
        }

        private void ClearForm()
        {
            TxtSpotNumber.Clear();
            CmbZone.SelectedIndex = -1;
            CmbSpotType.SelectedIndex = 0;
            CmbStatus.SelectedIndex = 0;
            GridSpots.SelectedItem = null;
        }
    }
}
