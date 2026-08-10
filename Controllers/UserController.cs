using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopCoAPI.Data;
using ShopCoAPI.DTO;
using ShopCoAPI.DTOs;
using ShopCoAPI.Models;
using ShopCoAPI.Services;
using BCrypt.Net;
using ShopCoAPI.Service;

namespace ShopCoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ShopCoDBContext _Context;
        private readonly ITokenService _TokenService;
        private readonly IEmailService _EmailService;
        private readonly ILogger<UsersController> _Logger;

        public UsersController(ShopCoDBContext context, ITokenService tokenService, IEmailService emailService, ILogger<UsersController> logger)
        {
            _Context = context;
            _TokenService = tokenService;
            _EmailService = emailService;
            _Logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> Register(UserRegisterDto registerDto)
        {
            if (registerDto is null)
                return BadRequest();

            var emailExists = await _Context.Users
                .AnyAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());

            if (emailExists)
                return BadRequest();

            var newUser = new Users
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Role = "Customer"
            };

            newUser.Password = new PasswordHasher<Users>()
                .HashPassword(newUser, registerDto.Password);

            _Context.Users.Add(newUser);
            await _Context.SaveChangesAsync();

            UserResponseDto response;
            try
            {
                response = new UserResponseDto
                {
                    Id = newUser.Id,
                    Name = newUser.Name,
                    Email = newUser.Email,
                    Role = newUser.Role,
                    AccessToken = _TokenService.GenerateToken(newUser),
                    RefreshToken = await GenerateAndSaveRefreshToken(newUser),
                    ProfilePictureUrl = newUser.ProfilePictureUrl
                };
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error generating tokens during register for user {Email}", newUser.Email);
                return StatusCode(500, "Failed to generate authentication tokens.");
            }

            return CreatedAtAction(nameof(GetUserProfile), new { id = response.Id }, response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> Login(UserLoginDto loginDto)
        {
            if (loginDto is null)
                return BadRequest();

            // 1. Attempt to find matching user credentials
            var user = await _Context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

            if (user is null)
                return Unauthorized();

            var verificationResult = new PasswordHasher<Users>()
                .VerifyHashedPassword(user, user.Password, loginDto.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
                return Unauthorized();

            // 2. Compile response state for local app tracking blocks
            try
            {
                var response = new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    AccessToken = _TokenService.GenerateToken(user),
                    RefreshToken = await GenerateAndSaveRefreshToken(user),
                    ProfilePictureUrl = user.ProfilePictureUrl
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error generating tokens during login for user {Email}", user.Email);
                return StatusCode(500, "Failed to generate authentication tokens.");
            }
        }

        [HttpPost("profile-picture")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UploadProfilePicture([FromForm] IFormFile file)
        {
            if (!Request.HasFormContentType)
            {
                var actual = Request.ContentType ?? "(none)";
                _Logger.LogWarning("UploadProfilePicture: unsupported content type {ContentType}", actual);
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, $"Expected multipart/form-data content type. Actual: {actual}");
            }

            if (file is null || file.Length == 0)
                return BadRequest();

            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(file.ContentType))
            {
                _Logger.LogWarning("UploadProfilePicture: unsupported image content type {ImageContentType}", file.ContentType);
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, $"Unsupported image content type. Allowed: image/jpeg, image/png, image/webp. Actual: {file.ContentType}");
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = await _Context.Users.FindAsync(userId);
            if (user is null)
                return NotFound();

            var url = await HttpContext.RequestServices.GetRequiredService<IR2StorageService>().UploadFileAsync(file);
            user.ProfilePictureUrl = url;
            await _Context.SaveChangesAsync();

            return Ok(new { Url = url });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            if (request is null)
                return BadRequest();

            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
                return Unauthorized();

            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                AccessToken = _TokenService.GenerateToken(user),
                RefreshToken = await GenerateAndSaveRefreshToken(user),
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return Ok(response);
        }

        [HttpGet("profile/{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserProfile(int id)
        {
            var user = await _Context.Users.FindAsync(id);
            if (user is null)
                return NotFound();

            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            if (dto is null || string.IsNullOrEmpty(dto.ResetEmail))
                return BadRequest("Email is required.");

            // 1. Find the user
            var user = await _Context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.ResetEmail.ToLower());

            // 2. Security best practice: Return success even if email doesn't exist
            // This prevents malicious users from guessing registered emails (Email Harvesting)
            if (user is null)
            {
                return Ok(new { message = "If the email exists, an OTP has been sent." });
            }

            // 3. Generate the 6-digit OTP
            string otp = Random.Shared.Next(100000, 999999).ToString();

            // 4. Save OTP and Expiry to DB (setting it to expire in 10 minutes)
            user.OTPCode = otp;
            user.OTPExpiryTime = DateTime.UtcNow.AddMinutes(10);
            await _Context.SaveChangesAsync();

            // 5. Send the real email!
            await _EmailService.SendEmailAsync(
                user.Email,
                "Your ShopCo Password Reset OTP",
                $"<h3>Password Reset Request</h3>" +
                $"<p>You requested an OTP to reset your password. Use the 6-digit code below to proceed:</p>" +
                $"<h2 style='color: #2b6cb0; letter-spacing: 2px;'>{otp}</h2>" +
                $"<p>This code is valid for 10 minutes.</p>"
            );

            return Ok(new { message = "If the email exists, an OTP has been sent." });
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            if (dto is null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.OTPCode))
                return BadRequest("Email and OTP code are required.");

            // 1. Find the user by email
            var user = await _Context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            // 2. If user doesn't exist, or the OTP is incorrect, or it has expired
            if (user is null || user.OTPCode != dto.OTPCode || user.OTPExpiryTime < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired OTP." });
            }

            // 3. Success! The OTP is verified.

            // 1. Invalidate the OTP so it can't be reused
            user.OTPCode = null;
            user.OTPExpiryTime = null;

            // 2. Save the cleared values to the database
            await _Context.SaveChangesAsync();

            return Ok(new { message = "OTP verified successfully. You may now reset your password." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (dto is null || string.IsNullOrEmpty(dto.Email))
                return BadRequest("Invalid request.");

            // 1. Verify passwords match
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            // 2. Find the user
            var user = await _Context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user is null)
            {
                return BadRequest("User not found.");
            }

            // 3. Hash the new password using the same PasswordHasher used in Register
            user.Password = new PasswordHasher<Users>().HashPassword(user, dto.NewPassword);
            await _Context.SaveChangesAsync();

            return Ok(new { message = "Password has been reset successfully." });
        }

        private async Task<Users?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await _Context.Users.FindAsync(userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            return user;
        }

        private async Task<string> GenerateAndSaveRefreshToken(Users user)
        {
            var refreshToken = _TokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _Context.SaveChangesAsync();
            return refreshToken;
        }
    }
}
