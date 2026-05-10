using SmartParking.Models;

namespace SmartParking.Services
{
    // Печать квитанций через Microsoft Word
    // Нужно установить: COM-ссылка на Microsoft.Office.Interop.Word
    public class WordPrinter
    {
        public void PrintReceipt(ParkingSession session)
        {
            // Используем dynamic чтобы не нужно было добавлять COM-ссылку напрямую
            // Это работает через позднее связывание
            try
            {
                // Создаём объект Word
                Type wordType = Type.GetTypeFromProgID("Word.Application")!;
                dynamic wordApp = Activator.CreateInstance(wordType)!;
                wordApp.Visible = true;

                // Создаём новый документ
                dynamic doc = wordApp.Documents.Add();
                dynamic selection = wordApp.Selection;

                // Заголовок
                selection.Font.Size = 16;
                selection.Font.Bold = true;
                selection.Font.Name = "Arial";
                selection.ParagraphFormat.Alignment = 1; // по центру
                selection.TypeText("КВИТАНЦИЯ ОБ ОПЛАТЕ ПАРКОВКИ");
                selection.TypeParagraph();
                selection.TypeParagraph();

                // Номер квитанции
                selection.Font.Size = 11;
                selection.Font.Bold = false;
                selection.ParagraphFormat.Alignment = 1;
                selection.TypeText($"№ {session.Id:D6}  от {DateTime.Now:dd.MM.yyyy HH:mm}");
                selection.TypeParagraph();
                selection.TypeParagraph();

                // Горизонтальная линия - просто тире
                selection.ParagraphFormat.Alignment = 0; // по левому краю
                selection.TypeText("------------------------------------------------");
                selection.TypeParagraph();

                // Данные о водителе и машине
                selection.Font.Bold = true;
                selection.TypeText("ДАННЫЕ ТРАНСПОРТНОГО СРЕДСТВА:");
                selection.TypeParagraph();
                selection.Font.Bold = false;

                if (session.Vehicle != null)
                {
                    selection.TypeText($"Гос. номер:      {session.Vehicle.LicensePlate}");
                    selection.TypeParagraph();
                    selection.TypeText($"Марка/модель:    {session.Vehicle.Brand} {session.Vehicle.Model}");
                    selection.TypeParagraph();

                    if (session.Vehicle.Driver != null)
                    {
                        selection.TypeText($"Владелец:        {session.Vehicle.Driver.FullName}");
                        selection.TypeParagraph();
                    }
                }

                selection.TypeParagraph();

                // Данные о парковке
                selection.Font.Bold = true;
                selection.TypeText("ДАННЫЕ О ПАРКОВКЕ:");
                selection.TypeParagraph();
                selection.Font.Bold = false;

                if (session.Spot != null)
                {
                    selection.TypeText($"Зона:            {session.Spot.Zone?.Name ?? "—"}");
                    selection.TypeParagraph();
                    selection.TypeText($"Место №:         {session.Spot.SpotNumber}");
                    selection.TypeParagraph();
                    selection.TypeText($"Адрес:           {session.Spot.Zone?.Address ?? "—"}");
                    selection.TypeParagraph();
                }

                selection.TypeText($"Въезд:           {session.EntryTime:dd.MM.yyyy HH:mm}");
                selection.TypeParagraph();
                selection.TypeText($"Выезд:           {(session.ExitTime.HasValue ? session.ExitTime.Value.ToString("dd.MM.yyyy HH:mm") : "ещё стоит")}");
                selection.TypeParagraph();
                selection.TypeText($"Время стоянки:   {session.GetHours():F1} ч.");
                selection.TypeParagraph();
                selection.TypeParagraph();

                // Итоговая сумма
                selection.TypeText("------------------------------------------------");
                selection.TypeParagraph();

                selection.Font.Bold = true;
                selection.Font.Size = 13;
                selection.TypeText($"Сумма к оплате:  {session.AmountDue ?? 0:F2} руб.");
                selection.TypeParagraph();

                if (session.FineAmount > 0)
                {
                    selection.TypeText($"Штраф:           {session.FineAmount:F2} руб.");
                    selection.TypeParagraph();
                    decimal total = (session.AmountDue ?? 0) + session.FineAmount;
                    selection.TypeText($"ИТОГО:           {total:F2} руб.");
                    selection.TypeParagraph();
                }

                selection.Font.Bold = false;
                selection.Font.Size = 11;

                if (session.IsPaid)
                {
                    selection.TypeText($"Оплачено:        {session.AmountPaid ?? 0:F2} руб.  ✓ ОПЛАЧЕНО");
                }
                else
                {
                    selection.TypeText("Статус:          НЕ ОПЛАЧЕНО");
                }

                selection.TypeParagraph();
                selection.TypeParagraph();
                selection.TypeText("------------------------------------------------");
                selection.TypeParagraph();

                // Подпись
                selection.Font.Size = 9;
                selection.ParagraphFormat.Alignment = 1;
                selection.TypeText("АСУ «Умная парковка города»");
                selection.TypeParagraph();
                selection.TypeText("Данная квитанция является документом об оплате.");
                selection.TypeParagraph();

                // Печатаем
                doc.PrintOut();

                Console.WriteLine("Квитанция отправлена на печать");
            }
            catch (Exception ex)
            {
                // Если Word не установлен или что-то пошло не так
                throw new Exception($"Ошибка при печати: {ex.Message}");
            }
        }

        // Просто открыть квитанцию в Word без печати (для просмотра)
        public void PreviewReceipt(ParkingSession session)
        {
            try
            {
                Type wordType = Type.GetTypeFromProgID("Word.Application")!;
                dynamic wordApp = Activator.CreateInstance(wordType)!;
                wordApp.Visible = true; // Показываем Word

                dynamic doc = wordApp.Documents.Add();
                dynamic selection = wordApp.Selection;

                // Заполняем так же как и PrintReceipt но не печатаем
                selection.Font.Size = 16;
                selection.Font.Bold = true;
                selection.Font.Name = "Arial";
                selection.ParagraphFormat.Alignment = 1;
                selection.TypeText("КВИТАНЦИЯ ОБ ОПЛАТЕ ПАРКОВКИ");
                selection.TypeParagraph();
                selection.TypeParagraph();

                selection.Font.Size = 11;
                selection.Font.Bold = false;
                selection.ParagraphFormat.Alignment = 1;
                selection.TypeText($"№ {session.Id:D6}  от {DateTime.Now:dd.MM.yyyy HH:mm}");
                selection.TypeParagraph();
                selection.TypeParagraph();

                selection.ParagraphFormat.Alignment = 0;
                selection.TypeText("------------------------------------------------");
                selection.TypeParagraph();

                selection.Font.Bold = true;
                selection.TypeText("ДАННЫЕ ТРАНСПОРТНОГО СРЕДСТВА:");
                selection.TypeParagraph();
                selection.Font.Bold = false;

                if (session.Vehicle != null)
                {
                    selection.TypeText($"Гос. номер:      {session.Vehicle.LicensePlate}");
                    selection.TypeParagraph();
                    selection.TypeText($"Марка/модель:    {session.Vehicle.Brand} {session.Vehicle.Model}");
                    selection.TypeParagraph();

                    if (session.Vehicle.Driver != null)
                    {
                        selection.TypeText($"Владелец:        {session.Vehicle.Driver.FullName}");
                        selection.TypeParagraph();
                    }
                }

                selection.TypeParagraph();
                selection.Font.Bold = true;
                selection.TypeText("ДАННЫЕ О ПАРКОВКЕ:");
                selection.TypeParagraph();
                selection.Font.Bold = false;

                if (session.Spot != null)
                {
                    selection.TypeText($"Зона:            {session.Spot.Zone?.Name ?? "—"}");
                    selection.TypeParagraph();
                    selection.TypeText($"Место №:         {session.Spot.SpotNumber}");
                    selection.TypeParagraph();
                }

                selection.TypeText($"Въезд:           {session.EntryTime:dd.MM.yyyy HH:mm}");
                selection.TypeParagraph();
                selection.TypeText($"Выезд:           {(session.ExitTime.HasValue ? session.ExitTime.Value.ToString("dd.MM.yyyy HH:mm") : "ещё стоит")}");
                selection.TypeParagraph();
                selection.TypeText($"Время стоянки:   {session.GetHours():F1} ч.");
                selection.TypeParagraph();
                selection.TypeParagraph();

                selection.TypeText("------------------------------------------------");
                selection.TypeParagraph();

                selection.Font.Bold = true;
                selection.Font.Size = 13;
                selection.TypeText($"Сумма к оплате:  {session.AmountDue ?? 0:F2} руб.");
                selection.TypeParagraph();

                if (session.FineAmount > 0)
                {
                    selection.TypeText($"Штраф:           {session.FineAmount:F2} руб.");
                    selection.TypeParagraph();
                }

                selection.Font.Bold = false;
                selection.Font.Size = 11;

                if (session.IsPaid)
                    selection.TypeText("Статус:          ✓ ОПЛАЧЕНО");
                else
                    selection.TypeText("Статус:          НЕ ОПЛАЧЕНО");

                selection.TypeParagraph();
                selection.TypeParagraph();
                selection.TypeText("------------------------------------------------");
                selection.TypeParagraph();
                selection.Font.Size = 9;
                selection.ParagraphFormat.Alignment = 1;
                selection.TypeText("АСУ «Умная парковка города»");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при открытии квитанции в Word: {ex.Message}");
            }
        }
    }
}
