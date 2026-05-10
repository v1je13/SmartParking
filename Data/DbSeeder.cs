using SmartParking.Models;

namespace SmartParking.Data
{
    // Начальные данные для базы данных
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            // Если уже есть данные - не добавляем
            if (db.ParkingZones.Any()) return;

            // Добавляем парковочные зоны
            var zone1 = new ParkingZone
            {
                Name = "Центральная",
                Address = "ул. Ленина, 1",
                ZoneType = "Платная",
                TotalSpots = 50,
                PricePerHour = 100,
                WorkingHours = "00:00-24:00",
                IsActive = true
            };

            var zone2 = new ParkingZone
            {
                Name = "Торговый центр Галерея",
                Address = "пр. Мира, 15",
                ZoneType = "Платная",
                TotalSpots = 200,
                PricePerHour = 60,
                WorkingHours = "09:00-23:00",
                IsActive = true
            };

            var zone3 = new ParkingZone
            {
                Name = "Подземная у вокзала",
                Address = "пл. Вокзальная, 1",
                ZoneType = "Подземная",
                TotalSpots = 80,
                PricePerHour = 150,
                WorkingHours = "06:00-00:00",
                IsActive = true
            };

            db.ParkingZones.AddRange(zone1, zone2, zone3);
            db.SaveChanges();

            // Добавляем места для первой зоны
            for (int i = 1; i <= 10; i++)
            {
                db.ParkingSpots.Add(new ParkingSpot
                {
                    ParkingZoneId = zone1.Id,
                    SpotNumber = $"A{i}",
                    SpotType = "Обычное",
                    Status = i <= 3 ? SpotStatus.Occupied : SpotStatus.Free
                });
            }

            // Несколько мест для инвалидов
            db.ParkingSpots.Add(new ParkingSpot
            {
                ParkingZoneId = zone1.Id,
                SpotNumber = "И1",
                SpotType = "Для инвалидов",
                Status = SpotStatus.Free
            });

            // Места для второй зоны
            for (int i = 1; i <= 20; i++)
            {
                db.ParkingSpots.Add(new ParkingSpot
                {
                    ParkingZoneId = zone2.Id,
                    SpotNumber = $"Б{i}",
                    SpotType = "Обычное",
                    Status = i % 3 == 0 ? SpotStatus.Occupied : SpotStatus.Free
                });
            }

            db.SaveChanges();

            // Добавляем водителей
            var driver1 = new Driver
            {
                LastName = "Иванов",
                FirstName = "Иван",
                MiddleName = "Иванович",
                Phone = "+7 (999) 123-45-67",
                Email = "ivanov@mail.ru",
                DriverLicense = "77АА 123456",
                RegisteredAt = DateTime.Now.AddDays(-100)
            };

            var driver2 = new Driver
            {
                LastName = "Петрова",
                FirstName = "Мария",
                MiddleName = "Сергеевна",
                Phone = "+7 (999) 987-65-43",
                Email = "petrova@gmail.com",
                DriverLicense = "50ВВ 654321",
                RegisteredAt = DateTime.Now.AddDays(-50)
            };

            var driver3 = new Driver
            {
                LastName = "Сидоров",
                FirstName = "Алексей",
                MiddleName = "Петрович",
                Phone = "+7 (903) 555-11-22",
                DriverLicense = "99КК 111222",
                RegisteredAt = DateTime.Now.AddDays(-200),
                HasDebt = true
            };

            db.Drivers.AddRange(driver1, driver2, driver3);
            db.SaveChanges();

            // Добавляем машины
            var car1 = new Vehicle
            {
                DriverId = driver1.Id,
                LicensePlate = "А123БВ77",
                Brand = "Toyota",
                Model = "Camry",
                Color = "Белый",
                Year = 2020,
                VehicleType = "Легковой"
            };

            var car2 = new Vehicle
            {
                DriverId = driver1.Id,
                LicensePlate = "О999ПР77",
                Brand = "BMW",
                Model = "X5",
                Color = "Чёрный",
                Year = 2022,
                VehicleType = "Легковой"
            };

            var car3 = new Vehicle
            {
                DriverId = driver2.Id,
                LicensePlate = "Е456МН50",
                Brand = "Hyundai",
                Model = "Solaris",
                Color = "Серый",
                Year = 2019,
                VehicleType = "Легковой"
            };

            var car4 = new Vehicle
            {
                DriverId = driver3.Id,
                LicensePlate = "Т777УФ99",
                Brand = "Lada",
                Model = "Granta",
                Color = "Синий",
                Year = 2018,
                VehicleType = "Легковой"
            };

            db.Vehicles.AddRange(car1, car2, car3, car4);
            db.SaveChanges();

            // Добавляем несколько сессий парковки
            var spot1 = db.ParkingSpots.First(s => s.Status == SpotStatus.Occupied && s.ParkingZoneId == zone1.Id);

            db.ParkingSessions.Add(new ParkingSession
            {
                VehicleId = car1.Id,
                ParkingSpotId = spot1.Id,
                EntryTime = DateTime.Now.AddHours(-3),
                Status = SessionStatus.Active
            });

            // Добавим завершённую сессию
            db.ParkingSessions.Add(new ParkingSession
            {
                VehicleId = car3.Id,
                ParkingSpotId = db.ParkingSpots.First(s => s.ParkingZoneId == zone2.Id).Id,
                EntryTime = DateTime.Now.AddDays(-1),
                ExitTime = DateTime.Now.AddDays(-1).AddHours(2),
                AmountDue = 120,
                AmountPaid = 120,
                IsPaid = true,
                Status = SessionStatus.Completed
            });

            // Добавим сессию с долгом
            db.ParkingSessions.Add(new ParkingSession
            {
                VehicleId = car4.Id,
                ParkingSpotId = db.ParkingSpots.Skip(2).First(s => s.ParkingZoneId == zone1.Id).Id,
                EntryTime = DateTime.Now.AddDays(-2),
                ExitTime = DateTime.Now.AddDays(-2).AddHours(5),
                AmountDue = 500,
                AmountPaid = 0,
                IsPaid = false,
                Status = SessionStatus.Unpaid
            });

            db.SaveChanges();
        }
    }
}
