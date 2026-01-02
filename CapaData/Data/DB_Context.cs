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
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<JobEntity> Jobs { get; set; } = null!;

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

                entity.Property(e => e.Iconos)
                      .HasMaxLength(200)
                      .IsRequired(false);


                entity.HasData(
                    new Menu { Id = 1, DescripcionMenu = "Administracion", IdPadre = null, Attach = "" },
                    new Menu { Id = 2, DescripcionMenu = "Roles", IdPadre = 1, Attach = "Roles/Index" },
                    new Menu { Id = 3, DescripcionMenu = "Usuarios", IdPadre = 1, Attach = "UserRoles/Index" },
                    new Menu { Id = 4, DescripcionMenu = "Menus", IdPadre = 1, Attach = "Menus/Index" }
                       );
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.Sku).IsRequired().HasMaxLength(40);
                e.Property(p => p.Name).IsRequired().HasMaxLength(200);
                e.Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
                e.Property(p => p.CurrentPrice).HasColumnType("decimal(18,2)");
                e.HasIndex(p => p.Sku).IsUnique();
            });

            modelBuilder.Entity<Promotion>(e =>
            {
                e.HasKey(p => p.Id);
                e.Property(p => p.ProductSku).IsRequired().HasMaxLength(40);
                e.Property(p => p.DiscountPercent).HasColumnType("decimal(9,4)");
                e.HasIndex(p => new { p.ProductSku, p.StartUtc, p.EndUtc });
            });


            var e = modelBuilder.Entity<JobEntity>();
            e.ToTable("Jobs");
            e.HasKey(x => x.Id);

            e.Property(x => x.Created)
             .HasColumnType("datetimeoffset(7)")
             .IsRequired();

            e.Property(x => x.Reason)
             .HasMaxLength(100)
             .IsRequired();

            e.Property(x => x.State)
             .HasMaxLength(20)
             .IsRequired();

            e.Property(x => x.Error)
             .HasMaxLength(4000);


            e.Property(j => j.TaskName).HasMaxLength(100).IsRequired();
            e.Property(j => j.StoredProc).HasMaxLength(128).IsRequired();



            base.OnModelCreating(modelBuilder);
        }
    }

}
