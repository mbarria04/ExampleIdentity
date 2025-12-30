
using CapaAplicacion.Interfaces;
using CapaAplicacion.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion
{
    using CapaAplicacion.Interfaces;
    using CapaData.Interfaces;

    public class Dependencias
    {
        public IMenuServices MenuServices { get; }

        public ICliente _Cliente { get; }

        public IProducto ProductoServices { get; }

        public Ipromocion PromocionServices { get; }

        public Dependencias(IMenuServices menuServices, ICliente cliente, IProducto producto, Ipromocion promocion )
        {
            MenuServices = menuServices;
            _Cliente = cliente;
            ProductoServices = producto;
            PromocionServices = promocion;
        }
    }

}
