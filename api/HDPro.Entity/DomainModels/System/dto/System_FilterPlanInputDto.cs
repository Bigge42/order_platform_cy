using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Entity.DomainModels.System.dto
{
    public class System_FilterPlanInputDto
    {
        public long? ID { get; set; }
        public string BillName { get; set; }
        public string Name { get; set; }

        public string Content { get; set; }
    }
}
