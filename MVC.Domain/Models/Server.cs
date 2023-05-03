﻿using System;
using System.Collections.Generic;

namespace MVC.Domain.Models
{
    public partial class Server
    {
        public int Id { get; set; }
        public string ServerName { get; set; } = null!;
        public string Channels { get; set; } = null!;
        public string Usernames { get; set; } = null!;
    }
}