namespace ShopCoAPI.Services.AiChat
{
    public interface IAiContextService
    {
        Task<string> BuildCustomerContextAsync(int userId);
        Task<string> BuildAdminContextAsync();
    }
}