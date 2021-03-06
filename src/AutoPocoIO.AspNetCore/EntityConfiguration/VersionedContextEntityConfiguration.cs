﻿using AutoPocoIO.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AutoPocoIO.EntityConfiguration
{
    internal class VersionedContextEntityConfiguration : IContextEntityConfiguration
    {
        public void SetupAppDbContext(ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            modelBuilder.ApplyConfiguration(new ConnectorConfiguration());
            modelBuilder.ApplyConfiguration(new UserJoinConfiguration());
        }

        public void SetupLogDbContext(ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            modelBuilder.ApplyConfiguration(new RequestLogConfiguration());
            modelBuilder.ApplyConfiguration(new ResponseLogConfiguration());

        }
    }
}
