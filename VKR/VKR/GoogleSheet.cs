using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Threading.Tasks;

namespace VKR
{
    public class GoogleSheet
    {
        public async Task WriteToSheet()
        {
            // Путь к вашему JSON-файлу с ключами
            string[] Scopes = { SheetsService.Scope.Spreadsheets };
            string ApplicationName = "VKR";
            var credential = GoogleCredential.FromFile("M:\\Users\\defaultuser0\\Documents\\Visual Studio\\Mine\\VKR\\vkr-99949-be677cbe8f7c.json")
                .CreateScoped(Scopes);

            // Создание службы Sheets API
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // ID вашей таблицы и диапазон для записи данных
            var spreadsheetId = "1y6vTqJ_TW4tRJA5HyAfZr91vQ78DHAW2PHfncFhzL-M";
            var range = "Sheet1!A2"; // Укажите нужный диапазон

            // Данные для записи
            var valueRange = new ValueRange();
            var oblist = new[] { new[] { "Hello, World!" } }; // Данные для записи
            valueRange.Values = oblist;

            // Запись данных в таблицу
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            var response = await updateRequest.ExecuteAsync();

            //Console.WriteLine("Data written to sheet: " + response.UpdatedCells + " cells updated.");
        }
    }
}