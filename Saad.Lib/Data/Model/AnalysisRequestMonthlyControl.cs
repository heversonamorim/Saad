using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class AnalysisRequestMonthlyControl {

        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int TotalRequestsGenerated { get; set; }
        public int ErrorsCount { get; set; }

    }
}
