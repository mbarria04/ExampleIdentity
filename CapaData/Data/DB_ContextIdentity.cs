using CapaData.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Data
{
    public class DB_ContextIdentity : IdentityDbContext<ApplicationUser>
    {
        public DB_ContextIdentity(DbContextOptions<DB_ContextIdentity> options)
            : base(options)
        {
        }
    }

}
