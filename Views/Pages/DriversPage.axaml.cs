using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using SmartParking.Data;
using SmartParking.Models;
using System;
using System.Linq;

namespace SmartParking.Views.Pages
{
    public partial class DriversPage : UserControl
    {
        private AppDbContext _db;
        private Driver? _editingDriver;
        private Vehicle? _editingVehicle;

        public DriversPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            LoadDrivers();
            LoadVehicles();
            ClearForm();
        }

        private void LoadDrivers()
        {
            var drivers = _db.Drivers.ToList();
            GridDrivers.ItemsSource = drivers;
        }

        private void LoadVehicles()
        {
            var vehicles = _db.Vehicles.Include(v => v.Driver).ToList();
            GridVehicles.ItemsSource = vehicles;
        }

        private void BtnAddDriver_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _editingDriver = null;
            _editingVehicle = null;
            ClearForm();
        }

        private void BtnEditDriver_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridDrivers.SelectedItem is Driver driver)
            {
                _editingDriver = driver;
                _editingVehicle = null;
                TxtLastName.Text = driver.LastName;
                TxtFirstName.Text = driver.FirstName;
                TxtMiddleName.Text = driver.MiddleName;
                TxtPhone.Text = driver.Phone;
                TxtLicensePlate.Clear();
                TxtBrand.Clear();
                TxtModel.Clear();
            }
        }

        private void BtnAddCar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _editingDriver = null;
            _editingVehicle = null;
            ClearForm();
        }

        private void BtnEditCar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridVehicles.SelectedItem is Vehicle vehicle)
            {
                _editingVehicle = vehicle;
                _editingDriver = null;
                TxtLastName.Text = vehicle.Driver?.LastName ?? "";
                TxtFirstName.Text = vehicle.Driver?.FirstName ?? "";
                TxtMiddleName.Text = vehicle.Driver?.MiddleName ?? "";
                TxtPhone.Text = vehicle.Driver?.Phone ?? "";
                TxtLicensePlate.Text = vehicle.LicensePlate;
                TxtBrand.Text = vehicle.Brand;
                TxtModel.Text = vehicle.Model;
            }
        }

        private void BtnSaveDriver_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtLastName.Text))
            {
                System.Console.WriteLine("Укажите фамилию!");
                return;
            }

            if (_editingDriver == null)
            {
                _editingDriver = new Driver();
                _db.Drivers.Add(_editingDriver);
            }

            _editingDriver.LastName = TxtLastName.Text;
            _editingDriver.FirstName = TxtFirstName.Text;
            _editingDriver.MiddleName = TxtMiddleName.Text;
            _editingDriver.Phone = TxtPhone.Text;

            _db.SaveChanges();
            LoadDrivers();
            ClearForm();
            System.Console.WriteLine("✓ Водитель сохранён!");
        }

        private void BtnDeleteDriver_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridDrivers.SelectedItem is Driver driver)
            {
                try
                {
                    _db.Drivers.Remove(driver);
                    _db.SaveChanges();
                    LoadDrivers();
                    LoadVehicles();
                    System.Console.WriteLine("✓ Водитель удалён!");
                }
                catch
                {
                    System.Console.WriteLine("✗ Не удалось удалить водителя!");
                }
            }
        }

        private void BtnDeleteCar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GridVehicles.SelectedItem is Vehicle vehicle)
            {
                try
                {
                    _db.Vehicles.Remove(vehicle);
                    _db.SaveChanges();
                    LoadVehicles();
                    System.Console.WriteLine("✓ Машина удалена!");
                }
                catch
                {
                    System.Console.WriteLine("✗ Не удалось удалить машину!");
                }
            }
        }

        private void BtnRefreshDrivers_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LoadDrivers();
        }

        private void BtnRefreshCars_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            LoadVehicles();
        }

        private void BtnCancelEdit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _editingDriver = null;
            _editingVehicle = null;
            ClearForm();
        }

        private void ClearForm()
        {
            TxtLastName.Clear();
            TxtFirstName.Clear();
            TxtMiddleName.Clear();
            TxtPhone.Clear();
            TxtLicensePlate.Clear();
            TxtBrand.Clear();
            TxtModel.Clear();
            GridDrivers.SelectedItem = null;
            GridVehicles.SelectedItem = null;
        }
    }
}
