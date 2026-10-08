namespace ChessAdmin.Components.Models
{
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class LoginResponseModel
    {
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = true;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
    }
}
