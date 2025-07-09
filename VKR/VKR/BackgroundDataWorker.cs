using System;
using System.Diagnostics;
using System.Timers;
using VKR.Models.ViewModels;
using VKR.Models.Entities;
using VKR.Controllers;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Threading.Tasks;

namespace VKR
{
    public static class BackgroundWorker
    {
        private static Timer _timer;
        private static bool _isRunning;

        public static void Start()
        {
            if (_isRunning) return;

            _timer = new Timer(60000); // Интервал 60 секунд
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            _isRunning = true;
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            string readAppPath = "M:\\Users\\defaultuser0\\Documents\\Python\\pythonProject\\dist\\main.exe";

            // Запускаем внешний процесс
            //Process.Start(readAppPath)?.WaitForExit();

            // Чтение данных
            //string[] data = ReadData();

            // Сохраняем данные в базу
            //SaveDataToDatabase(data);

            //Task.Run(async () => await WriteToSheet());

            //using (var context = new VKREntities())
            //{
            //    if (context.Data.Count() == 51)
            //        Archieve();
            //}
        }

        //public static void Archieve()
        //{
        //    bool done = false;
        //    try
        //    {
        //        string path = "M:\\Users\\defaultuser0\\Documents\\Visual Studio\\Mine\\VKR\\PreparedData.txt";
        //        StreamWriter sw = new StreamWriter(path);
        //        sw.WriteLine("1");

        //        List<Datum> data = new List<Datum>();
        //        using (var context = new VKREntities())
        //        {
        //            data = context.Data.ToList();
        //        }

        //        string toRecord = "";
        //        for (int index = 1; index < data.Count; index++)
        //        {
        //            toRecord = data[index].Number.ToString() + "-" +
        //                data[index].Datetime.ToString() + "-" +
        //                data[index].LM75A.ToString() + "-" +
        //                data[index].DHT22.ToString() + "-" +
        //                data[index].BMP280.ToString();
        //            sw.WriteLine(toRecord);
        //        }

        //        sw.Close();

        //        string readAppPath = "M:\\Users\\defaultuser0\\Documents\\Visual Studio\\Mine\\VKR\\VKRSupport\\VKRSupport\\bin\\Debug\\net5.0\\VKRSupport.exe";
        //        Process.Start(readAppPath)?.WaitForExit();

        //        done = true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Exception: " + e.Message);
        //    }
        //    finally
        //    {//}
        //        Console.WriteLine("Executing finally block.");
        //    }

        //    if (done == true)
        //        ClearData();
        

        //private static void ClearData()
        //{
        //    Datum last;
        //    using (var context = new VKREntities())
        //    {
        //        last = context.Data.OrderByDescending(d => d.Datetime).FirstOrDefault();
        //        foreach (var data in context.Data)
        //        {
        //            context.Entry(data).State = System.Data.Entity.EntityState.Deleted;
        //        }
        //        context.SaveChanges();
        //    }

        //    using (var context = new VKREntities())
        //    {
        //        context.Data.Add(last);
        //        context.SaveChanges();
        //    }
        //}

        private static string[] ReadData()
        {
            string[] data = new string[3];
            try
            {
                string path = "M:\\Users\\defaultuser0\\Documents\\Python\\UIRS\\Data.txt";
                using (var sr = new StreamReader(path))
                {
                    string line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        data = line.Split(' ');
                        for (int i = 0; i < data.Length; i++)
                        {
                            data[i] = data[i].Replace('.', ',');
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }

            return data;
        }

        private static void SaveDataToDatabase(string[] data)
        {
            using (var context = new VKREntities())
            {
                DateTime dm = DateTime.Now;
                Datum last = context.Data.OrderByDescending(d => d.ID).FirstOrDefault();
                var dataVM = new DataVM
                {
                    Datetime = dm,
                    LM75A = double.Parse(data[0]),
                    BMP280 = double.Parse(data[1]),
                    DHT22 = double.Parse(data[2])
                };

                var datumTemp = new Datum
                {
                    ID = last.ID + 1,
                    DateTime = dataVM.Datetime,
                    Transducer_ID = 1,
                    Value = (double)dataVM.LM75A
                };

                var datumHumid = new Datum
                {
                    ID = datumTemp.ID + 1,
                    DateTime = dataVM.Datetime,
                    Transducer_ID = 2,
                    Value = (double)dataVM.DHT22
                };

                var datumPress = new Datum
                {
                    ID = datumHumid.ID + 1,
                    DateTime = dataVM.Datetime,
                    Transducer_ID = 3,
                    Value = (double)dataVM.BMP280
                };

                context.Data.Add(datumTemp);
                context.Data.Add(datumHumid);
                context.Data.Add(datumPress);
                context.SaveChanges();
            }
        }

        public static async Task WriteToSheet()
        {
            string[] Scopes = { SheetsService.Scope.Spreadsheets };
            string ApplicationName = "VKR";
            string path = @"M:\Users\defaultuser0\Documents\Visual Studio\Mine\VKR\vkr-99949-0bd29d92dd64.json";

            try
            {
                var credential = GoogleCredential.FromFile(path)
                .CreateScoped(Scopes);

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                var spreadsheetId = "1y6vTqJ_TW4tRJA5HyAfZr91vQ78DHAW2PHfncFhzL-M";
                var range = "Sheet1!A2:D2"; // Диапазон, охватывающий все четыре ячейки
                
                List<DataVM> dataVMs = new List<DataVM>();
                using (var context = new VKREntities())
                {
                    List<Datum> data = context.Data.OrderByDescending(d => d.DateTime).Take(3).ToList();
                    for (int index = 1; index <= data.Count; index += 3)
                    {
                        DataVM newDataVM = new DataVM();
                        newDataVM.Datetime = data[index].DateTime;
                        newDataVM.LM75A = data[index - 1].Value;
                        newDataVM.BMP280 = data[index + 1].Value;
                        newDataVM.DHT22 = data[index].Value;
                        dataVMs.Add(newDataVM);
                    }
                }

                var valueRange = new ValueRange();
                valueRange.Values = new List<IList<object>>
    {
        new List<object> { dataVMs[0].Datetime.ToString(), dataVMs[0].LM75A, dataVMs[0].DHT22, dataVMs[0].BMP280 }
    };

                var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

                await updateRequest.ExecuteAsync();
            }
            catch (Exception ex)
            {
                string e = ex.Message.ToString();
            }
        }
    }
}