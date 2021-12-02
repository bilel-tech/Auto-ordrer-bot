using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_ordrer_bot.Models
{
    public class Config
    {
        public string ECommerceSite { get; set; }
        public string ECommerceSiteUserName { get; set; }
        public string ECommerceSitePassWord { get; set; }
        public string UrlProduct { get; set; }
        public string CardNbr { get; set; }
        public DateTime CardExPiry { get; set; }
        public string CardCVV { get; set; }
        public string CardName { get; set; }
        public int RepeatCount { get; set; }
        public string EmailUserName { get; set; }
        public string EmailPassword { get; set; }
        public string Cancel { get; set; }
        public string ZipCode { get; set; }
    }
}
