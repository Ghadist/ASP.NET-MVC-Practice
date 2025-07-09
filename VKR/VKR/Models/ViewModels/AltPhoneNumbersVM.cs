using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace VKR.Models.ViewModels
{
    public class AltPhoneNumbersVM
    {
        [Required]
        [DisplayName("Имя")]
        public string Name { get; set; }

        public byte phone_ID { get; set; }

        [Required]
        [DisplayName("Номер")]
        public string Number { get; set; }

        [DisplayName("Заметка")]
        public string Note { get; set; }

        public List<SelectListItem> Addressees { get; set; }
    }
}