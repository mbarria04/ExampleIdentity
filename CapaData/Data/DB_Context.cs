using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaData.Entities;

namespace CapaData.Data
{
    public partial class DB_Context : DbContext
    {
        public DB_Context(DbContextOptions<DB_Context> options) : base(options)
        {
        }

        public DbSet<Menu> Menus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menus");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.DescripcionMenu)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Attach)
                      .HasMaxLength(500)
                     .IsRequired(false);

                entity.Property(e => e.RolesUsers)
                     .HasMaxLength(500)
                    .IsRequired(false);

                entity.HasOne<Menu>()
                      .WithMany()
                      .HasForeignKey(e => e.IdPadre)
                      .OnDelete(DeleteBehavior.Restrict); // importante para evitar cascadas

                entity.HasData(
                    new Menu { Id = 1, DescripcionMenu = "Administracion", IdPadre = null, Attach = "" },
                    new Menu { Id = 2, DescripcionMenu = "Roles", IdPadre = 1, Attach = "Roles/Index" },
                    new Menu { Id = 3, DescripcionMenu = "Usuarios", IdPadre = 1, Attach = "UserRoles/Index" },
                    new Menu { Id = 4, DescripcionMenu = "Menus", IdPadre = 1, Attach = "Menus/Index" }
                       );
            });



            base.OnModelCreating(modelBuilder);
        }
    }

}
