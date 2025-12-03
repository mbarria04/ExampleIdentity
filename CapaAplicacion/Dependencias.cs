
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

    public class Dependencias
    {
        public IMenuServices MenuServices { get; }

        public Dependencias(IMenuServices menuServices)
        {
            MenuServices = menuServices;
        }
    }

}
