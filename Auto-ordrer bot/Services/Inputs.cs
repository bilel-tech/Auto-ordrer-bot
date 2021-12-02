using Auto_ordrer_bot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_ordrer_bot.Services
{
    public static class Inputs
    {
        public static List<Config> FlipKartOrders { get; set; }
        public static List<Config> AmazonOrders { get; set; }
    }
}
