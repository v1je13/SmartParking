using Microsoft.EntityFrameworkCore;
using SmartParking.Models;

namespace SmartParking.Data
{
    public class AppDbContext : DbContext
    {
        // Таблицы в базе данных
        public DbSet<ParkingZone> ParkingZones { get; set; }
        public DbSet<ParkingSpot> ParkingSpots { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ParkingSession> ParkingSessions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Строка подключения к SQL Server - поменяй на свою если надо
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=SmartParkingDB;Trusted_Connection=True;"
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настраиваем связи

            // Один водитель - много машин
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Driver)
                .WithMany(d => d.Vehicles)
                .HasForeignKey(v => v.DriverId);

            // Одна зона - много мест
            modelBuilder.Entity<ParkingSpot>()
                .HasOne(s => s.Zone)
                .WithMany(z => z.Spots)
                .HasForeignKey(s => s.ParkingZoneId);

            // Одна сессия - одна машина
            modelBuilder.Entity<ParkingSession>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.Sessions)
                .HasForeignKey(s => s.VehicleId);

            // Одна сессия - одно место
            modelBuilder.Entity<ParkingSession>()
                .HasOne(s => s.Spot)
                .WithMany(p => p.Sessions)
                .HasForeignKey(s => s.ParkingSpotId);

            // Точность для decimal полей
            modelBuilder.Entity<ParkingZone>()
                .Property(z => z.PricePerHour)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ParkingSession>()
                .Property(s => s.AmountDue)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ParkingSession>()
                .Property(s => s.AmountPaid)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ParkingSession>()
                .Property(s => s.FineAmount)
                .HasPrecision(10, 2);
        }
    }
}
