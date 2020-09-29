using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;


namespace AutoPocoIO.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            modelBuilder.ApplyConfiguration(new ConnectorConfiguration());
            modelBuilder.ApplyConfiguration(new UserJoinEntityConfiguration());

            base.OnModelCreating(modelBuilder);

            modelBuilder.Seed();
        }

        public DbSet<Connector> Connector { get; set; }
        public DbSet<UserJoin> UserJoin { get; set; }
    }
}