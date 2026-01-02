using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Models
{

    public record PriceJob(
          Guid Id,
          DateTimeOffset Created,
          string Reason,
          string TaskName = "RecalculatePrices",
          string StoredProc = "dbo.sp_RecalculatePricesForValidPromotions"
      );

}
