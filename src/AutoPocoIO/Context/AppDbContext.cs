using AutoPocoIO.Extensions;
using AutoPocoIO.Models;
using Microsoft.EntityFrameworkCore;


namespace AutoPocoIO.Context
{
    internal class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.CreateModel();

            base.OnModelCreating(modelBuilder);

            modelBuilder.Seed();
        }

        public DbSet<Connector> Connector { get; set; }
        public DbSet<UserJoin> UserJoin { get; set; }
    }
}