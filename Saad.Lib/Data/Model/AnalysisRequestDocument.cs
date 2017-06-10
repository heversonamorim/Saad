using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class AnalysisRequestDocument {

        public int Id { get; set; }

        public string FileName { get; set; }
        
        public string Url { get; set; }

        public int ReferenceMonth { get; set; }

        public int ReferenceYear { get; set; }

        [NotMapped]
        public string ReferenceDate {
            get {
                if (ReferenceMonth > 0 && ReferenceYear > 0) {
                    return string.Format("{0:00}/{1}", ReferenceMonth, ReferenceYear);
                }
                return string.Empty;
            }
        }

        public string EvaluationResult { get; set; }

        public int? EvaluationPoints { get; set; }

        public int? MaximumEvaluationPoints { get; set; }

        public string EvaluationReservations { get; set; }

        public string EvaluationRemarks { get; set; }

        public virtual AnalysisRequest AnalysisRequest { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }

        public string Relevance { get; set; }

        public string DocumentReference { get; set; }

        [NotMapped]
        public bool IsPresent { get { return !string.IsNullOrWhiteSpace(Url); } }

        public string ReasonNotPresent { get; set; }

    }
}
