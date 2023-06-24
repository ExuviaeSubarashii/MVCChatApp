using System;
using System.Collections.Generic;

namespace MVC.Domain.Models
{
    public partial class MessageDto
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = null!;
        public DateTime SenderTime { get; set; }
        public string Message1 { get; set; } = null!;
        public string Server { get; set; } = null!;
        public string Channel { get; set; } = null!;
        public string? ImageDir { get; set; }
    }
}
