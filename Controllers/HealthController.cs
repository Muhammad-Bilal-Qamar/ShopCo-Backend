using Microsoft.AspNetCore.Mvc;

namespace ShopCoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private static readonly DateTime StartedAtUtc = DateTime.UtcNow;

        // GET /api/health
        [HttpGet]
        public IActionResult Get()
        {
            var uptime = DateTime.UtcNow - StartedAtUtc;

            return Ok(new
            {
                status = "healthy",
                timestampUtc = DateTime.UtcNow,
                uptimeSeconds = Math.Floor(uptime.TotalSeconds),
                uptime = uptime.ToString(@"dd\.hh\:mm\:ss"),
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }
    }
}