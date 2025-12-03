using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Entities
{
    public class VM_MenuItems
    {
        public int Id { get; set; }
        public string DescripcionMenu { get; set; }
        public int? IdPadre { get; set; }
        public string? Attach { get; set; }

        public string? Iconos { get; set; }

        public List<VM_MenuItems> Hijos { get; set; } = new List<VM_MenuItems>();

        public bool IsActive { get; set; }
    }
}
