using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PdfManagement.API.HealthChecks
{
    public class DbContextHealthCheck<TContext> : IHealthCheck where TContext : DbContext
    {
        private readonly TContext _dbContext;

        public DbContextHealthCheck(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if the database connection is working
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                
                if (canConnect)
                {
                    return HealthCheckResult.Healthy("Database connection is healthy");
                }
                
                return HealthCheckResult.Unhealthy("Cannot connect to the database");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database health check failed", ex);
            }
        }
    }
}
