using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Data
{
    public class DB_ContextFactoryGlobal : IDesignTimeDbContextFactory<DB_Context>
    {
        public DB_Context CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("IdentityConnection");

            var optionsBuilder = new DbContextOptionsBuilder<DB_Context>();
            optionsBuilder.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("CapaData");
                sqlOptions.EnableRetryOnFailure();
                sqlOptions.CommandTimeout(180);
            });

            return new DB_Context(optionsBuilder.Options);
        }
    }

}
