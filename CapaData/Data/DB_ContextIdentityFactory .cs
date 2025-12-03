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
    public class DB_ContextIdentityFactory : IDesignTimeDbContextFactory<DB_ContextIdentity>
    {
        public DB_ContextIdentity CreateDbContext(string[] args)
        {
            // Asegúrate que la ruta sea correcta desde este proyecto hacia appsettings.json del proyecto de inicio
            var configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false)
                   .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DB_ContextIdentity>();
            var connectionString = configuration.GetConnectionString("IdentityConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new DB_ContextIdentity(optionsBuilder.Options);
        }
    }

}
