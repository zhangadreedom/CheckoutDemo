using CheckoutDemo.Domain.Payments.Entities;
using Microsoft.EntityFrameworkCore;

namespace CheckoutDemo.Infrastructure.Persistence.EF
{
    public sealed class CheckoutDbContext : DbContext
    {
        public CheckoutDbContext(DbContextOptions<CheckoutDbContext> options)
            : base(options)
        {
        }

        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CheckoutDbContext).Assembly);
        }
    }
}
