using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Entities
{
    public class DataTableRequestData
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public SearchData Search { get; set; }
    }

    public class SearchData
    {
        public string Value { get; set; }
    }
}
