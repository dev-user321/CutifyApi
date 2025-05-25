using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RepositoryLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TokenCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var expiredTokens = db.RefreshTokens.Where(t => t.ExpiryDate <= DateTime.UtcNow);
                db.RefreshTokens.RemoveRange(expiredTokens);
                await db.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); // Hər 15 saniyədə bir təmizlə
            }
        }
    }

}
