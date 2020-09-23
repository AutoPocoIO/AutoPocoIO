using AutoPocoIO.EntityConfiguration;
using AutoPocoIO.Exceptions;
using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;


namespace AutoPocoIO.Context
{
    public class AppDbContext : DbContext
    {
        private readonly IContextEntityConfiguration _config;

        public AppDbContext(DbContextOptions<AppDbContext> options, IContextEntityConfiguration config) : base(options)
        {
            _config = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            _config.SetupAppDbContext(modelBuilder);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Seed();
        }

        public DbSet<Connector> Connector { get; set; }
        public DbSet<UserJoin> UserJoin { get; set; }
    }
}