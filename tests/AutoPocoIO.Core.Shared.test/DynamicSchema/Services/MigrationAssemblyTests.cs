using AutoPocoIO.DynamicSchema.Services.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoPocoIO.test.DynamicSchema.Services
{
    [TestClass]
    [TestCategory(TestCategories.Unit)]
    public class MigrationAssemblyTests
    {
        private DbContextOptions dbOptions;
        private MigrationsAssembly migrationAsm;
        private List<Assembly> createdAssemblies;
        private List<Type> createTypes;

        [TestInitialize]
        public void Init()
        {
            createTypes = new List<Type>();
            dbOptions = new DbContextOptionsBuilder()
                .UseSqlite("DataSource=appDb" + Guid.NewGuid().ToString())
                .Options;

            createdAssemblies = new List<Assembly>();
        }

        private void CreateMigrationAsmFinder()
        {
            var currentContext = new Mock<ICurrentDbContext>();
            currentContext.Setup(c => c.Context).Returns(new AsmMigrationContext1());
            migrationAsm = new MigrationsAssembly(currentContext.Object,
                                                      dbOptions,
                                                      Mock.Of<IMigrationsIdGenerator>(),
                                                      Mock.Of<IDiagnosticsLogger<DbLoggerCategory.Migrations>>());

            //Filter out becuase other test asms might be in the list and waiting to be unloaded
            PrivateObject obj = new PrivateObject(migrationAsm);
            var asms = (Assembly[]) obj.GetField("_autoPocoAssemblies");
            asms = asms.Where(c => createdAssemblies.Contains(c)).ToArray();
            obj.SetField("_autoPocoAssemblies", asms);


        }

       
        public class AsmMigrationBase : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                throw new NotImplementedException();
            }
        }
        public class AsmMigrationContextBase : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase(databaseName: "appDb" + Guid.NewGuid().ToString());
            }
        }

        public class AsmMigration1 : AsmMigrationBase
        { }
    

        public class AsmMigrationContext1 : AsmMigrationContextBase
        { }

  
        private ModuleBuilder CreateMigration(string migrationId, ModuleBuilder moduleBuilder = null)
        {
            if (moduleBuilder == null)
            {
                AssemblyName an = new AssemblyName("AUTOPOCOIO.name1_" + Guid.NewGuid());
                AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
                moduleBuilder = builder.DefineDynamicModule("module1");

                createdAssemblies.Add(moduleBuilder.Assembly);
            }


            var migrationTypeBuilder = moduleBuilder.DefineType("typeMig_"+ migrationId, TypeAttributes.Public | TypeAttributes.Class, typeof(AsmMigrationBase));
            var migrationType = migrationTypeBuilder.CreateType();
            createTypes.Add(migrationType);

            //force asm to load
            var active = Activator.CreateInstance(migrationType);

            TypeDescriptor.AddAttributes(migrationType,
               new DbContextAttribute(typeof(AsmMigrationContext1)),
               new MigrationAttribute(migrationId)
               );

            return moduleBuilder;

        }
    
        [TestMethod]
        public void GetSingleMigrationForSingleAsm()
        {
            string migratinId = "Migration_" + Guid.NewGuid();

            CreateMigration(migratinId);
            CreateMigrationAsmFinder();

            var results = migrationAsm.Migrations;

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(migratinId, results.Keys.First());
            Assert.AreEqual(createTypes.First(), results[migratinId]);
           
        }

        [TestMethod]
        public void OnlyCheckAssembliesOnces()
        {
            string migratinId = "Migration_" + Guid.NewGuid();
            CreateMigration(migratinId);
            CreateMigrationAsmFinder();

            var results = migrationAsm.Migrations;
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(migratinId, results.Keys.First());


            TypeDescriptor.AddAttributes(typeof(AsmMigration1),
               new MigrationAttribute(migratinId + "_replaced")
               );


            //Check if attibute changes but collectoin does not
            var results2 = migrationAsm.Migrations;
            Assert.AreEqual(1, results2.Count());
            Assert.AreEqual(migratinId, results2.Keys.First());
            Assert.AreEqual(migratinId + "_replaced", TypeDescriptor.GetAttributes(typeof(AsmMigration1)).OfType<MigrationAttribute>().First().Id);
        }

        [TestMethod]
        public void GetMultipleMigrationForSingleAsm()
        {
            string migratinId = "Migration_" + Guid.NewGuid();
            string migratinId2 = "Migration2_" + Guid.NewGuid();
            var module = CreateMigration(migratinId);
            CreateMigration(migratinId2, module);
            CreateMigrationAsmFinder();

            var results = migrationAsm.Migrations;

            Assert.AreEqual(2, results.Count());
            Assert.AreEqual(migratinId, results.Keys.First());
            Assert.AreEqual("typeMig_"+ migratinId, results[migratinId].Name);

            Assert.AreEqual(migratinId2, results.Keys.Last());
            Assert.AreEqual("typeMig_" + migratinId2, results[migratinId2].Name);

        }

        [TestMethod]
        public void GetMultipleMigrationForMultipleAsm()
        {
            string migratinId = "Migration_" + Guid.NewGuid();
            string migratinId2 = "Migration2_" + Guid.NewGuid();
            CreateMigration(migratinId);
            CreateMigration(migratinId2);
            CreateMigrationAsmFinder();

            var results = migrationAsm.Migrations;

            Assert.AreEqual(2, results.Count());
            Assert.AreEqual(migratinId, results.Keys.First());
            Assert.AreEqual("typeMig_" + migratinId, results[migratinId].Name);

            Assert.AreEqual(migratinId2, results.Keys.Last());
            Assert.AreEqual("typeMig_" + migratinId2, results[migratinId2].Name);

        }

        [TestMethod]
        public void IgnoreNullTypes()
        {
            string migratinId = "Migration_" + Guid.NewGuid();
            var module = CreateMigration(migratinId);
            module.DefineType("type2");
            CreateMigrationAsmFinder();

            var results = migrationAsm.Migrations;
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(migratinId, results.Keys.First());

        }

        [TestMethod]
        public void SkipIfMissingMigrationAttribute()
        {
            string migratinId = "Migration_" + Guid.NewGuid();
            var module = CreateMigration(migratinId);
            var secondType = module.DefineType("type2");
            var migrationType = secondType.CreateType();
            createTypes.Add(migrationType);

            TypeDescriptor.AddAttributes(migrationType,
               new DbContextAttribute(typeof(AsmMigrationContext1))
               );


            CreateMigrationAsmFinder();

            var results = migrationAsm.Migrations;
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(migratinId, results.Keys.First());
        }

        [TestMethod]
        public void SkipIfDifferentContextTypeAttribute()
        {
            string migratinId = "Migration_" + Guid.NewGuid();
            var module = CreateMigration(migratinId);

            var secondType = module.DefineType("type2");
            var migrationType = secondType.CreateType();
            createTypes.Add(migrationType);

            TypeDescriptor.AddAttributes(migrationType,
               new DbContextAttribute(typeof(int)),
                new MigrationAttribute("skipThis")
               );


            CreateMigrationAsmFinder();

            var results = migrationAsm.Migrations;
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(migratinId, results.Keys.First());
        }
    }
}
