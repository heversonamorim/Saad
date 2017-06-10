using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class AnalysisRequestActivity {

        public int Id { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Remarks { get; set; }
        public virtual AnalysisRequest AnalysisRequest { get; set; }

    }
}
