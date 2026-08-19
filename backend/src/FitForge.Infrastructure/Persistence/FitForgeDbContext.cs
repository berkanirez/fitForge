using Microsoft.EntityFrameworkCore;

namespace FitForge.Infrastructure.Persistence;

public class FitForgeDbContext : DbContext
{
    public FitForgeDbContext(DbContextOptions<FitForgeDbContext> options)
        : base(options)
    {
    }
}
