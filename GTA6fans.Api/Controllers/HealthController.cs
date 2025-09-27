using Microsoft.AspNetCore.Mvc;
using GTA6fans.Infrastructure.Data;
using MongoDB.Bson;

namespace GTA6fans.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly MongoDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(MongoDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Route("/health")]
    public Task<IActionResult> Get()
    {
        try
        {
            // Check if the API is running
            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "GTA6fans.Api",
                Version = "1.0.0"
            };

            return Task.FromResult<IActionResult>(Ok(healthStatus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult<IActionResult>(StatusCode(500, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Service = "GTA6fans.Api",
                Version = "1.0.0",
                Error = ex.Message
            }));
        }
    }

    [HttpGet]
    [Route("/health/detailed")]
    public async Task<IActionResult> GetDetailed()
    {
        try
        {
            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "GTA6fans.Api",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                Database = await CheckDatabaseHealth()
            };

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            return StatusCode(500, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Service = "GTA6fans.Api",
                Version = "1.0.0",
                Error = ex.Message
            });
        }
    }

    private async Task<object> CheckDatabaseHealth()
    {
        try
        {
            // Ping the database
            var command = new BsonDocument("ping", 1);
            await _context.Database.RunCommandAsync<BsonDocument>(command);
            
            return new
            {
                Status = "Connected",
                DatabaseName = _context.Database.DatabaseNamespace.DatabaseName,
                Message = "Database connection is healthy"
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed");
            return new
            {
                Status = "Disconnected",
                Message = "Database connection failed",
                Error = ex.Message
            };
        }
    }
}