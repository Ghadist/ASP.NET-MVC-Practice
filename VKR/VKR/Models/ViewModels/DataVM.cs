using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VKR.Models.ViewModels
{
    public class DataVM
    {
        public System.DateTime Datetime { get; set; }
        public Nullable<double> LM75A { get; set; }
        public Nullable<double> DHT22 { get; set; }
        public Nullable<double> BMP280 { get; set; }
    }
}