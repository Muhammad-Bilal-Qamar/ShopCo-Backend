using ShopCoAPI.Models;

namespace ShopCoAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(Users user);

        string GenerateRefreshToken();
    }
}
