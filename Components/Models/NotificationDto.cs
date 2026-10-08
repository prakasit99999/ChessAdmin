using System;
using System.Collections.Generic;

namespace ChessAdmin.Components.Models
{
    public class NotificationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "info"; // info, success, warning, error
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class NotificationResponseDto
    {
        public bool Success { get; set; }
        public int UnreadCount { get; set; }
        public int Total { get; set; }
        public List<NotificationDto> Data { get; set; } = new();
    }

    public class UnreadCountResponseDto
    {
        public bool Success { get; set; }
        public int UnreadCount { get; set; }
    }
}
