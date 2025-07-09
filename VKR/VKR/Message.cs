using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VKR
{
    public class Message
    {
        public string Channel { get; set; }
        public string To { get; set; }
        public string Content { get; set; }
    }
}