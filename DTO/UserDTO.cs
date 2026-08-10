using System.ComponentModel.DataAnnotations;

namespace ShopCoAPI.DTOs
{
    public class UserRegisterDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 4)]
        public string Password { get; set; } = string.Empty;
    }

    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string ResetEmail { get; set; } = string.Empty;
    }
}