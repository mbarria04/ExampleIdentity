using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion.DTOs
{
    public class DataTableRequestDto
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public SearchDto Search { get; set; }
    }

    public class SearchDto
    {
        public string Value { get; set; }
    }
}
