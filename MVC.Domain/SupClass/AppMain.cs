using MVC.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC.Domain.SupClass
{
    public static class AppMain
    {
        public static User User { get; set; } = null;
        public static Message Message { get; set; } = null;
        public static Server Servers { get; set; } = null;
        public static DirectMessages DirectMessages { get; set; } = null;

    }
}
