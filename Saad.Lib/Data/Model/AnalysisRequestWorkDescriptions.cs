using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class AnalysisRequestWorkDescriptions {
        public int Id { get; set; }
        public virtual AnalysisRequest Request { get; set; }
        public virtual WorkDescription WorkDescription { get; set; }
    }
}
