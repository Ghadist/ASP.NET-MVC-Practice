using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using VKR.Models.Entities;
using VKR.Models.ViewModels;
using VKR;

public class SmsController : Controller
{
    private SmsService _smsService;

    public SmsController()
    {
        _smsService = new SmsService("_qV9wBuJTNa1YVoXZA-B9Q==");
    }

    [HttpGet]
    [Authorize(Roles = "ADMINISTRATOR")]
    public ActionResult SendSms()
    {
        using (var context = new VKREntities())
        {
            var addressees = context.Addressees.Select(a => new SelectListItem
            {
                Value = a.ID.ToString(),
                Text = a.Name
            }).ToList();

            var model = new SmsVM
            {
                Addressees = addressees
            };

            return View(model);
        }
    }

    [HttpGet]
    public JsonResult GetPhoneNumbers(byte addresseeId)
    {
        using (var context = new VKREntities())
        {
            var phones = context.Addressee_Numbers
                .Where(an => an.Addressee_ID == addresseeId)
                .Select(an => new
                {
                    Id = an.Phone_Numbers.ID,
                    Value = an.Phone_Numbers.Value
                }).ToList();

            return Json(phones, JsonRequestBehavior.AllowGet);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> SendSms(SmsVM model)
    {
        using (var context = new VKREntities())
        {
            var number = context.Phone_Numbers
                .Where(p => p.ID == model.SelectedPhoneId)
                .Select(p => p.Value)
                .FirstOrDefault();

            if (number == null)
            {
                return Json(new { success = false, message = "Номер телефона не найден." });
            }

            Datum last = context.Data.OrderByDescending(d => d.DateTime).FirstOrDefault();

            var smsMessage = new Message
            {
                Channel = "sms",
                To = number,
                //Content = "Погодные данные на момент " + last.DateTime.ToString() +
                //": температура — " + last.LM75A.ToString() +
                //"°C, влажность — " + last.DHT22.ToString() +
                //"%, давление — " + last.BMP280.ToString() +
                //" мм. рт. ст. " + model.Message
            };

            var result = await _smsService.SendMessagesAsync(new List<Message> { smsMessage });

            return Json(new
            {
                success = result,
                message = result ? "SMS успешно отправлено!" : "Ошибка при отправке SMS."
            });
        }
    }

}