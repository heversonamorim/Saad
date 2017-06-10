using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class AnalysisRequestFeedback {

        public int Id { get; set; }

        public AnalysisRequest AnalysisRequest { get; set; }

        public string Type { get; set; }

        public string Feedback { get; set; }

        public DateTime CreateDate { get; set; }

        public ApplicationUser CreateUser { get; set; }

    }
}
