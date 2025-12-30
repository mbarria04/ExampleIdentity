using Autofac;
using CapaAplicacion.Interfaces;
using CapaAplicacion.Servicios;
using CapaData.Interfaces;
using CapaData.Repositorios;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion
{
    public static class FabricaServicios
    {
        public static void RegisterServices(ContainerBuilder builder, IConfiguration configuration)
        {

            builder.RegisterType<MenuRepositorio>().As<IMenuRepositorio>().InstancePerLifetimeScope();
            builder.RegisterType<ClienteRepositorio>().As<ICLienteRepositorio>().InstancePerLifetimeScope();
            builder.RegisterType<ProductoRepositorio>().As<IProductoRepositorio>().InstancePerLifetimeScope();
            builder.RegisterType<PromotionRepositorio>().As<IPromotionRepositorio>().InstancePerLifetimeScope();


            builder.RegisterType<MenuServices>()
                   .As<IMenuServices>()
                   .InstancePerLifetimeScope();

            builder.RegisterType<Cliente>()
                  .As<ICliente>()
                  .InstancePerLifetimeScope();

            builder.RegisterType<ProductoServices>()
                  .As<IProducto>()
                  .InstancePerLifetimeScope();

            builder.RegisterType<PromocionServices>()
                  .As<Ipromocion>()
                  .InstancePerLifetimeScope();



            builder.RegisterType<Dependencias>()
                   .AsSelf()
                   .InstancePerLifetimeScope();



        }
    }
}
