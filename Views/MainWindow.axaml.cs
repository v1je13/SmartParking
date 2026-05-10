using Avalonia.Controls;
using Avalonia.Threading;
using SmartParking.Data;
using SmartParking.Views.Pages;

namespace SmartParking.Views
{
    public partial class MainWindow : Window
    {
        private AppDbContext _db;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            // Создаём контекст базы данных
            _db = new AppDbContext();
            _db.Database.EnsureCreated();

            // Добавляем тестовые данные
            DbSeeder.Seed(_db);

            // Таймер для часов
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                TxtDateTime.Text = DateTime.Now.ToString("dd.MM.yyyy\nHH:mm:ss");
            };
            _timer.Start();

            // Показываем главную страницу
            ShowDashboard();
        }

        private void ShowDashboard()
        {
            MainContent.Content = new DashboardPage(_db);
            SetActiveButton(BtnDashboard);
        }

        private void BtnDashboard_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainContent.Content = new DashboardPage(_db);
            SetActiveButton(BtnDashboard);
        }

        private void BtnZones_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainContent.Content = new ZonesPage(_db);
            SetActiveButton(BtnZones);
        }

        private void BtnSpots_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainContent.Content = new SpotsPage(_db);
            SetActiveButton(BtnSpots);
        }

        private void BtnDrivers_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainContent.Content = new DriversPage(_db);
            SetActiveButton(BtnDrivers);
        }

        private void BtnSessions_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainContent.Content = new SessionsPage(_db);
            SetActiveButton(BtnSessions);
        }

        private void BtnReports_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainContent.Content = new ReportsPage(_db);
            SetActiveButton(BtnReports);
        }

        private void BtnPrint_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            MainContent.Content = new PrintPage(_db);
            SetActiveButton(BtnPrint);
        }

        // Подсвечиваем активную кнопку
        private void SetActiveButton(Button activeBtn)
        {
            var btns = new[] { BtnDashboard, BtnZones, BtnSpots, BtnDrivers, BtnSessions, BtnReports, BtnPrint };
            foreach (var btn in btns)
            {
                btn.Classes.Remove("active");
            }
            activeBtn.Classes.Add("active");
        }
    }
}
