using AutoPocoIO.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;

namespace AutoPocoIO.EntityConfiguration
{
    internal class VersionedContextEntityConfiguration : IContextEntityConfiguration
    {
        public void SetupAppDbContext(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.ApplyConfiguration(new ConnectorConfiguration());
            modelBuilder.ApplyConfiguration(new UserJoinConfiguration());
        }

        public void SetupLogDbContext(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.ApplyConfiguration(new RequestLogConfiguration());
            modelBuilder.ApplyConfiguration(new ResponseLogConfiguration());

        }
    }
}
