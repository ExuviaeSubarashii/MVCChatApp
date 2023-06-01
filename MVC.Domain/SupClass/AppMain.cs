using MVC.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC.Domain.SupClass
{
    public class AppMain
    {
        public static User User { get; set; }
        public static Message Message { get; set; }
        public static Server Servers { get; set; }
        public static DirectMessages DirectMessages { get; set; }

    }
}
