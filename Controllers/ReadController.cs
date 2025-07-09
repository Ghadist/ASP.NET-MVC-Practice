using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VKR.Models.Entities;
using VKR.Models.ViewModels;
using System.IO;
using System.Diagnostics;

namespace VKR.Controllers
{
    public class ReadController : Controller
    {
        // GET: Read
        public ActionResult Show()
        {
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
                return View(dataVMs[0]);
            }
        }

        public ActionResult GetLatestData()
        {
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
                return PartialView("LatestData", dataVMs[0]);
            }
        }

        [ChildActionOnly]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult ListOfNumbers()
        {
            List<PhoneNumbersVM> numbers = GetPNVM();
            return View(numbers);
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult CreateNumber()
        {
            PhoneNumbersVM phone = new PhoneNumbersVM();
            using (var context = new VKREntities())
            {
                var addressees = context.Addressees.Select(a => new SelectListItem
                {
                    Value = a.ID.ToString(),
                    Text = a.Name
                }).ToList();

                phone.Addressees = addressees;
            }
            return View(phone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNumber(PhoneNumbersVM model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new VKREntities())
                {
                    // Находим адресата по имени
                    var addressee = context.Addressees.FirstOrDefault(a => a.Name == model.Name);
                    if (addressee == null)
                    {
                        ModelState.AddModelError("Name", "Выбранный адресат не найден.");
                        model.Addressees = context.Addressees.Select(a => new SelectListItem
                        {
                            Value = a.Name,
                            Text = a.Name
                        }).ToList();
                        return View(model);
                    }

                    Phone_Numbers newPhone = new Phone_Numbers
                    {
                        ID = (byte)(context.Phone_Numbers.OrderByDescending(d => d.ID).FirstOrDefault()?.ID + 1 ?? 1),
                        Value = model.Number
                    };

                    Addressee_Numbers newAN = new Addressee_Numbers
                    {
                        Addressee_ID = addressee.ID,
                        Phone_ID = newPhone.ID,
                        Note = model.Note
                    };

                    context.Phone_Numbers.Add(newPhone);
                    context.Addressee_Numbers.Add(newAN);
                    context.SaveChanges();
                }

                return RedirectToAction("Show");
            }

            // Повторно загружаем список адресатов, если модель не валидна
            using (var context = new VKREntities())
            {
                model.Addressees = context.Addressees.Select(a => new SelectListItem
                {
                    Value = a.Name,
                    Text = a.Name
                }).ToList();
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult EditNumber(string phone)
        {
            List<PhoneNumbersVM> numbers = GetPNVM();
            PhoneNumbersVM targetRecord = numbers.Find(x => x.Number == phone);
            return View(targetRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNumber(PhoneNumbersVM model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new VKREntities())
                {
                    Addressee editedAddressee = context.Addressees.Find(model.addressee_ID);
                    Phone_Numbers editedNumber = context.Phone_Numbers.Find(model.phone_ID);
                    Addressee_Numbers editedAN = context.Addressee_Numbers.Find(model.phone_ID, model.addressee_ID);

                    editedAddressee.Name = model.Name;
                    editedNumber.Value = model.Number;
                    editedAN.Note = model.Note;

                    context.Addressees.Attach(editedAddressee);
                    context.Phone_Numbers.Attach(editedNumber);
                    context.Addressee_Numbers.Attach(editedAN);

                    context.Entry(editedAddressee).State = System.Data.Entity.EntityState.Modified;
                    context.Entry(editedNumber).State = System.Data.Entity.EntityState.Modified;
                    context.Entry(editedAN).State = System.Data.Entity.EntityState.Modified;

                    context.SaveChanges();
                }
                return RedirectToAction("Show");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult DeleteNumber(string phone)
        {
            List<PhoneNumbersVM> numbers = GetPNVM();
            PhoneNumbersVM targetRecord = numbers.Find(x => x.Number == phone);
            return View(targetRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteNumber(PhoneNumbersVM model)
        {
            using (var context = new VKREntities())
            {
                Phone_Numbers phoneTodelete = context.Phone_Numbers.Find(model.phone_ID);
                Addressee_Numbers anToDelete = context.Addressee_Numbers.Find(model.phone_ID, model.addressee_ID);

                context.Entry(anToDelete).State = System.Data.Entity.EntityState.Deleted;
                context.Entry(phoneTodelete).State = System.Data.Entity.EntityState.Deleted;
                context.SaveChanges();
            }
            return RedirectToAction("Show");
        }

        private List<PhoneNumbersVM> GetPNVM()
        {
            List<PhoneNumbersVM> numbers = new List<PhoneNumbersVM>();
            using (var db = new VKREntities())
            {
                List<Addressee_Numbers> ns = db.Addressee_Numbers.ToList();

                foreach (Addressee_Numbers n in ns)
                {
                    Addressee name = db.Addressees.Find(n.Addressee_ID);
                    Phone_Numbers phone = db.Phone_Numbers.Find(n.Phone_ID);

                    PhoneNumbersVM newPhone = new PhoneNumbersVM();
                    newPhone.Name = name.Name;
                    newPhone.Number = phone.Value;
                    newPhone.Note = n.Note;
                    newPhone.addressee_ID = n.Addressee_ID;
                    newPhone.phone_ID = n.Phone_ID;
                    numbers.Add(newPhone);
                }
            }
            return numbers;
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult ListOfAddressees()
        {
            List<Addressee> people;
            using (var context = new VKREntities())
            {
                people = context.Addressees.OrderBy(x => x.ID).ToList();
            }
            return View(people);
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult CreateAddressee()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAddressee(Addressee model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new VKREntities())
                {
                    Addressee newAddressee = new Addressee
                    {
                        ID = (byte)(context.Addressees.OrderByDescending(x => x.ID).FirstOrDefault().ID + 1),
                        Name = model.Name
                    };

                    context.Addressees.Add(newAddressee);
                    context.SaveChanges();
                }

                return RedirectToAction("ListOfAddressees");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult EditAddressee(byte idToEdit)
        {
            Addressee addresseeToEdit;
            using (var context = new VKREntities())
            {
                addresseeToEdit = context.Addressees.Find(idToEdit);
            }
            return View(addresseeToEdit);
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult DeleteAddressee(byte idToDelete)
        {
            Addressee addresseeToDelete;
            using (var context = new VKREntities())
            {
                addresseeToDelete = context.Addressees.Find(idToDelete);
            }
            return View(addresseeToDelete);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAddressee(Addressee model)
        {
            using (var context = new VKREntities())
            {
                Addressee addresseeToDelete = context.Addressees.Find(model.ID);
                List<Addressee_Numbers> anToDelete = context.Addressee_Numbers.Where(x => x.Addressee_ID == model.ID).ToList();
                List<Phone_Numbers> numbersToDelete = new List<Phone_Numbers>();

                foreach (Addressee_Numbers an in anToDelete)
                {
                    Phone_Numbers numberToDelete = context.Phone_Numbers.Find(an.Phone_ID);
                    numbersToDelete.Add(numberToDelete);
                }

                context.Entry(addresseeToDelete).State = System.Data.Entity.EntityState.Deleted;

                foreach (Addressee_Numbers an in anToDelete)
                    context.Entry(an).State = System.Data.Entity.EntityState.Deleted;

                foreach (Phone_Numbers phone in numbersToDelete)
                    context.Entry(phone).State = System.Data.Entity.EntityState.Deleted;

                context.SaveChanges();
            }
            return RedirectToAction("ListOfAddressees");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAddressee(Addressee model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new VKREntities())
                {
                    context.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("ListOfAddressees");
            }
            return View(model);
        }

        private void ClearData()
        {
            using (var context = new VKREntities())
            {
                foreach (var data in context.Data)
                {
                    context.Entry(data).State = System.Data.Entity.EntityState.Deleted;
                }
                context.SaveChanges();
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult BasicReview()
        {
            string path = "M:\\Users\\defaultuser0\\Documents\\Visual Studio\\Mine\\VKR\\PreparedData.txt";
            StreamWriter sw = new StreamWriter(path);
            sw.WriteLine("2");

            List<DataVM> dataVMs = new List<DataVM>();
            using (var context = new VKREntities())
            {
                List<Datum> data = context.Data.ToList();
                for(int index = 1; index <= data.Count; index += 3)
                {
                    DataVM newDataVM = new DataVM();
                    newDataVM.Datetime = data[index].DateTime;
                    newDataVM.LM75A = data[index - 1].Value;
                    newDataVM.BMP280 = data[index + 1].Value;
                    newDataVM.DHT22 = data[index].Value;
                    dataVMs.Add(newDataVM);
                }
            }

            string toRecord = "";
            for (int index = 1; index < dataVMs.Count; index++)
            {
                toRecord = 0.ToString() + "-" +
                    dataVMs[index].Datetime.ToString() + "-" +
                    dataVMs[index].LM75A.ToString() + "-" +
                    dataVMs[index].DHT22.ToString() + "-" +
                    dataVMs[index].BMP280.ToString();
                sw.WriteLine(toRecord);
            }

            sw.Close();

            string readAppPath = "M:\\Users\\defaultuser0\\Documents\\Visual Studio\\Mine\\VKR\\VKRSupport\\VKRSupport\\bin\\Debug\\net5.0\\VKRSupport.exe";
            Process.Start(readAppPath)?.WaitForExit();

            path = "M:\\Users\\defaultuser0\\Documents\\Visual Studio\\Mine\\VKR\\IsItDone.txt";
            StreamReader sr = new StreamReader(path);

            bool done = false;
            if (sr.ReadLine() == "Yes")
            {
                done = true;
            }

            return View(done);
        }

        [HttpGet]
        [Authorize(Roles = "ADMINISTRATOR")]
        public ActionResult Graphics()
        {
            //string path = "M:\\Users\\defaultuser0\\Documents\\Visual Studio\\Mine\\VKR\\PreparedData.txt";
            //StreamWriter sw = new StreamWriter(path);
            //sw.WriteLine("3");

            //List<Datum> data = new List<Datum>();
            //using (var context = new VKREntities())
            //{
            //    data = context.Data.ToList();
            //}

            //string toRecord = "";
            //for (int index = 1; index < data.Count; index++)
            //{
            //    toRecord = data[index].Number.ToString() + "-" +
            //        data[index].Datetime.ToString() + "-" +
            //        data[index].LM75A.ToString() + "-" +
            //        data[index].DHT22.ToString() + "-" +
            //        data[index].BMP280.ToString();
            //    sw.WriteLine(toRecord);
            //}

            //sw.Close();

            //string readAppPath = "M:\\Users\\defaultuser0\\Documents\\Visual Studio\\Mine\\VKR\\VKRSupport\\VKRSupport\\bin\\Debug\\net5.0\\VKRSupport.exe";
            //Process.Start(readAppPath)?.WaitForExit();

            return View();
        }

        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpPost]
        public ActionResult GetData(DateTime selectedDate)
        {
            List<DataVM> dataVMs = new List<DataVM>();
            using (var context = new VKREntities())
            {
                List<Datum> data = context.Data.ToList();
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

            List<DataVM> selected = dataVMs.Where(d => d.Datetime.Date == selectedDate.Date).ToList();

            return Json(new { data = selected });
        }
    }
}