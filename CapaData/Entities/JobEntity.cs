using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Entities
{
    public class JobEntity
    {

        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }     // datetimeoffset(7)
        public string Reason { get; set; } = null!;     // nvarchar(100)
        public string State { get; set; } = null!;      // nvarchar(20) (e.g. "Pending")
        public string? Error { get; set; }


        public string TaskName { get; set; } = "RecalculatePrices";
        public string StoredProc { get; set; } = "dbo.sp_RecalculatePricesForValidPromotions";


    }
}
