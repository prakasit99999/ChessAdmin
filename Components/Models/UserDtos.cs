using System.ComponentModel.DataAnnotations;

namespace ChessAdmin.Components.Models
{
    public class UserItem
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int Rating { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PagedUserResponse
    {
        public List<UserItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class UserCountsResponse
    {
        public int Online { get; set; }
        public int Offline { get; set; }
        public int Pending { get; set; }
    }

    public class CreateUserDto
    {
        [Required(ErrorMessage = "กรุณากรอก Username")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username ต้องมีความยาว 3-20 ตัวอักษร")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอก Email")]
        [EmailAddress(ErrorMessage = "รูปแบบ Email ไม่ถูกต้อง")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอก รหัสผ่าน")]
        [MinLength(6, ErrorMessage = "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอก ยืนยันรหัสผ่าน")]
        [Compare("Password", ErrorMessage = "รหัสผ่านและยืนยันรหัสผ่านไม่ตรงกัน")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AdminUpdateUserRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }

    }

    public class RegisterResponse
    {
        public string? Message { get; set; }
        public bool Success { get; set; }
    }

    public class UserProfileDto
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "กรุณากรอก Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอก Email")]
        [EmailAddress(ErrorMessage = "รูปแบบ Email ไม่ถูกต้อง")]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }

        public int Rating { get; set; } = 1500;
        public int Win { get; set; } = 0;
        public int Lose { get; set; } = 0;
        public int Drawn { get; set; } = 0;
    }

    // DTO รวมค่า Metric ด้านบนให้เรียกใช้ง่าย
    public class DashboardMetricsDto
    {
        public int TotalUsers { get; set; } = 1250;
        public int TotalGames { get; set; } = 4820;
        public int AvgMoveTimeMs { get; set; } = 85;
        public double PruningEfficiency { get; set; } = 92.4;
        public int SystemIssues { get; set; } = 1;
    }

    public class DashboardUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
        public bool IsActive { get; set; }
    }
}
