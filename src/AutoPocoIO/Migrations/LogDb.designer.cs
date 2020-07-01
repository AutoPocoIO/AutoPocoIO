﻿// <auto-generated />
using System;
using AutoPocoIO.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AutoPocoIO.Migrations
{
    [DbContext(typeof(LoggingMigrationContext))]
    [Migration(MigrationNames.LogDb)]
    partial class LogDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("AutoPoco.Logging.Models.RequestLog", b =>
                {
                    b.Property<long>("RequestId");

                    b.Property<Guid>("RequestGuid");

                    b.Property<string>("Connector")
                        .HasMaxLength(50);

                    b.Property<DateTime?>("DateTimeUtc")
                        .HasColumnType("datetime2(4)");

                    b.Property<DateTime?>("DayOfRequest")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasComputedColumnSql("CONVERT(date, DateTimeUtc)");

                    b.Property<string>("RequestType")
                        .HasMaxLength(10);

                    b.Property<string>("RequesterIp")
                        .HasMaxLength(39);

                    b.HasKey("RequestId", "RequestGuid");

                    b.HasIndex("DateTimeUtc");

                    b.HasIndex("DayOfRequest")
                        .HasName("IX_DayWithIP")
                        .HasAnnotation("SqlServer:Include", new[] { "RequesterIp" });

                    b.HasIndex("RequestGuid")
                        .HasAnnotation("SqlServer:Include", new[] { "DateTimeUtc", "RequestType", "RequesterIp", "Connector" });

                    b.HasIndex("DayOfRequest", "RequestType")
                        .HasName("IX_DayAndType");

                    b.ToTable("Request", "AutoPocoLog");
                });

            modelBuilder.Entity("AutoPoco.Logging.Models.ResponseLog", b =>
                {
                    b.Property<long>("ResponseId");

                    b.Property<Guid>("RequestGuid");

                    b.Property<DateTime?>("DateTimeUtc")
                        .HasColumnType("datetime2(4)");

                    b.Property<DateTime?>("DayOfResponse")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasComputedColumnSql("CONVERT(date, DateTimeUtc)");

                    b.Property<string>("Status")
                        .HasMaxLength(51);

                    b.Property<string>("Exception");

                    b.HasKey("ResponseId", "RequestGuid");

                    b.ToTable("Response", "AutoPocoLog");
                });
#pragma warning restore 612, 618
        }
    }
}
