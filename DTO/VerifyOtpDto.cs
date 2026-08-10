namespace ShopCoAPI.DTO
{
    public class VerifyOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string OTPCode { get; set; } = string.Empty;
    }
}
