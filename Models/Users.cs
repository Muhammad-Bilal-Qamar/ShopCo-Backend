namespace ShopCoAPI.Models
{
    public class Users
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }

        public string? OTPCode { get; set; }

        public DateTime? OTPExpiryTime { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? ProfilePictureUrl { get; set; }

    }
}
