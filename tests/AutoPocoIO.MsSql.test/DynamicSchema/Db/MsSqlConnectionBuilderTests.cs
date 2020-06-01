using AutoPocoIO.DynamicSchema.Enums;
using AutoPocoIO.MsSql.DynamicSchema.Db;
using AutoPocoIO.Resources;
using Xunit;

namespace AutoPocoIO.MsSql.test.DynamicSchema.Db
{

    [Trait("Category", TestCategories.Unit)]
    public class MsSqlConnectionBuilderTests
    {
        [FactWithName]
        public void ResourceTypeSet()
        {
            var builder = new MsSqlConnectionBuilder();
            Assert.Equal(ResourceType.Mssql, builder.ResourceType);
        }

        [FactWithName]
        public void CreateConnectionString()
        {
            var info = new ConnectionInformation()
            {
                InitialCatalog = "cat1",
                UserId = "user1",
                DataSource = "dt1",
                Password = "pass1",
            };

            var builder = new MsSqlConnectionBuilder();
            var results = builder.CreateConnectionString(info);

            Assert.Equal("Data Source=dt1;Initial Catalog=cat1;Persist Security Info=False;User ID=user1;Password=pass1;MultipleActiveResultSets=False;Connect Timeout=30;TrustServerCertificate=False", results);
        }

        [FactWithName]
        public void ParseConnectionString()
        {
            var connString = "Data Source=dt1;Initial Catalog=cat1;Persist Security Info=False;User ID=user1;Password=pass1;MultipleActiveResultSets=False;Connect Timeout=30;TrustServerCertificate=False";
            var builder = new MsSqlConnectionBuilder();
            var results = builder.ParseConnectionString(connString);

            Assert.Equal("cat1", results.InitialCatalog);
            Assert.Equal("user1", results.UserId);
            Assert.Equal("dt1", results.DataSource);
            Assert.Equal("pass1", results.Password);


        }
    }
}
