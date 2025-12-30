using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Entities
{
    public class Promotion
    {
        public int Id { get; set; }
        public string ProductSku { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; } // 0..1
        public DateTimeOffset StartUtc { get; set; }
        public DateTimeOffset EndUtc { get; set; }
    }
}
