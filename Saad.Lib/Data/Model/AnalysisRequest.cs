using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class AnalysisRequest {

        public const string Homologation = "Homologation";
        public const string MonthlyAnalysis = "MonthlyAnalysis";
        public const string Compliance = "Compliance";
        public const string Litigation = "Litigation";

        public AnalysisRequest() {
            Documents = new Collection<AnalysisRequestDocument>();
            Status = AnalysisRequestStatus.WaitingForDocuments;
            Activities = new Collection<AnalysisRequestActivity>();
            Feedbacks = new Collection<AnalysisRequestFeedback>();
            WorkDescriptions = new Collection<AnalysisRequestWorkDescriptions>();
        }

        public int Id { get; set; }

        [Required]
        public string Type { get; set; }

        [NotMapped]
        public string TypeName {
            get {
                if (Type == Homologation)
                    return "Homologação";
                if (Type == MonthlyAnalysis)
                    return "Análise Mensal";
                if (Type == Compliance)
                    return "Compliance";
                if (Type == Litigation)
                    return "Contencioso";
                return "Desconhecido";
            }
        }

        public virtual Supplier Supplier { get; set; }

        [NotMapped]
        public AnalysisRequestStatus Status { get; set; }

        public string RequestStatus {
            get {
                return Status.Value;
            }
            set {
                Status = AnalysisRequestStatus.Convert(value);
            }
        }

        public virtual Collection<AnalysisRequestDocument> Documents { get; set; }


        public DateTime? ContractStartDate {get;set;}
		public DateTime? ContractFinalDate {get;set;}

		public decimal? ContractTotalAmount {get;set;}
		public decimal? ContractBalance {get;set;}

        public string ContractFileName { get; set; }
        public string ContractFileUrl { get; set; }
        
        public virtual Work Work { get; set; }

        //public virtual WorkDescription WorkDescription { get; set; }
        public virtual Collection<AnalysisRequestWorkDescriptions> WorkDescriptions { get; set; }

        [StringLength(255, ErrorMessage="Local do serviço deve conter até 255 caracteres")]
        
        public string ServiceLocal { get; set; }

        [StringLength(255, ErrorMessage = "Descrição do serviço deve conter até 255 caracteres")]
        public string ServiceDescription { get; set; }

        public DateTime? ReferenceDate { get;set;}
        public decimal? EvaluationPercentage { get; set; }


        public DateTime CreateDate { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual ApplicationUser RequestedByUser { get; set; }

        public virtual Department RequestedByDepartment { get; set; }

        public virtual Collection<AnalysisRequestActivity> Activities { get; set; }

        public virtual Collection<AnalysisRequestFeedback> Feedbacks { get; set; }

        [NotMapped]
        public double Progress {
            get {
                double done = 1;

                if (Status.Equals(AnalysisRequestStatus.WaitingForAnalysis))
                    done++;

                if (Status.Equals(AnalysisRequestStatus.WaitingForFeedback))
                    done += 2;

                if (Status.IsAWorkflowEnd)
                    done += 3;

                return done / 4d;
            }
        }

        public virtual AnalysisRequestLitigation LitigationData { get; set; }

        public int? EmployeeQuantity { get; set; }

    }
}
