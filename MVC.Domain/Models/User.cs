using System;
using System.Collections.Generic;

namespace MVC.Domain.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Password { get; set; }
        public string? HasPassword { get; set; }
        public byte[]? Image { get; set; }
        public string? Server { get; set; }
        public string? Friends { get; set; }
        public string? FriendRequests { get; set; }
    }
}
