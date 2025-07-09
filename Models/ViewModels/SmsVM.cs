using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VKR.Models.ViewModels
{
    public class SmsVM
    {
        [DisplayName("Имя адресата")]
        public byte SelectedAddresseeId { get; set; }
        [DisplayName("Номер телефона")]
        public byte SelectedPhoneId { get; set; }
        [DisplayName("Текст сообщения к погодным данным")]
        public string Message { get; set; }

        public List<SelectListItem> Addressees { get; set; }
        public List<SelectListItem> PhoneNumbers { get; set; } = new List<SelectListItem>();
    }
}