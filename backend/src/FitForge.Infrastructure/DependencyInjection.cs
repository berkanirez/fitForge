using FitForge.Infrastructure.Identity;
using FitForge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FitForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FitForgeDb")
            ?? throw new InvalidOperationException("Connection string 'FitForgeDb' was not found.");

        services.AddDbContext<FitForgeDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<FitForgeDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
