using Microsoft.EntityFrameworkCore;

namespace finance_tracker.Models;
public class transactionsContext : DbContext
{
    public transactionsContext(DbContextOptions<transactionsContext> options) : base(options){}

    public DbSet<transactions> transactions {get;set;} = null!;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=finances;Trusted_Connection=True;TrustServerCertificate=True;");
    }
}