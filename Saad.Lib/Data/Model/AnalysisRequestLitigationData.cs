using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class AnalysisRequestLitigation {

        [ForeignKey("Request")]
        public int Id { get; set; }

        public virtual AnalysisRequest Request { get; set; }

        [DisplayName("Contrato Inicial")]
        [Required]
        public decimal InitialContractValue { get; set; }

        [DisplayName("Verba")]
        [Required]
        public decimal ContractSum { get; set; }

        [DisplayName("Resultado")]
        [NotMapped]
        public decimal Result { get { return ContractSum - InitialContractValue; } }

        [DisplayName("Adiantamento")]
        [Required]
        public decimal ContractAdvanceValue { get; set; }

        [DisplayName("Aditivos")]
        [Required]
        public decimal ContractAdditionsValue { get; set; }

        [DisplayName("Total do Contrato")]
        [NotMapped]
        public decimal ContractTotal { get { return ContractAdditionsValue + InitialContractValue; } }

        [Required]
        [DisplayName("Fornecido")]
        public decimal ContractSuppliedValue { get; set; }

        [DisplayName("Saldo Bloqueado")]
        [NotMapped]
        public decimal ContractBlockedValue { get { return ContractTotal - ContractSuppliedValue; } }

        [DisplayName("Caução Retida")]
        [Required]
        public decimal BondValue { get; set; }

        [DisplayName("Pedidos")]
        [Required]
        public string Orders { get; set; }

        [DisplayName("Pagamento Funcionários")]
        [Required]
        public decimal ContractEmployeesPayment { get; set; }

        [DisplayName("Pagamento Impostos")]
        [Required]
        public decimal TaxesPaymentValue { get; set; }

        [DisplayName("Pagamento Fornecedores")]
        [Required]
        public decimal SuppliersPaymentValue { get; set; }

        [DisplayName("Nova Contratação")]
        [Required]
        public decimal HiringValue { get; set; }

        [DisplayName("Total da Substituição")]
        [NotMapped]
        public decimal ReplacementTotal { get { return ContractEmployeesPayment + TaxesPaymentValue + SuppliersPaymentValue + HiringValue; } }

        [DisplayName("Saldo (Inicial x Final)")]
        [NotMapped]
        public decimal Balance { get { return ContractBlockedValue + BondValue - ReplacementTotal; } }


        #region Juridical Information

        [DisplayName("Processo")]
        [Required]
        public string ProcessCode { get; set; }

        [DisplayName("Vara")]
        [Required]
        public string JudgmentPlace { get; set; }

        [DisplayName("Comarca")]
        [Required]
        public string JudicialDistrict { get; set; }

        [DisplayName("Pendências para Ingresso")]
        [Required]
        public string PendenciesBlockingIngress { get; set; }

        [DisplayName("Data Andamento")]
        [Required]
        public DateTime? ProgressDate { get; set; }

        [DisplayName("Andamento")]
        [Required]
        public string ProgressDescription { get; set; }

        [DisplayName("Plano de Ação")]
        [Required]
        public string ActionPlan { get; set; }

        [DisplayName("Escritório Responsável")]
        [Required]
        public string ResponsibleLawyerOffice { get; set; }

        [DisplayName("Fase")]
        [Required]
        public string JudicialPhase { get; set; }

        [DisplayName("Fase")]
        [NotMapped]
        public string JudicialPhaseName {
            get {
                if (JudicialPhase == "Knowledge") return "Conhecimento";
                if (JudicialPhase == "Recursion") return "Recursão";
                if (JudicialPhase == "Execution") return "Execução";
                return "Desconhecido";
            }
        }

        [DisplayName("Tipo")]
        [Required]
        public string LitigationType { get; set; }

        [DisplayName("Tipo")]
        [NotMapped]
        public string LitigationTypeName {
            get {
                if (LitigationType == "Judicial") return "Judicial";
                if (LitigationType == "PreJudicial") return "Pré Judicial";
                if (LitigationType == "ExtraJudicial") return "Extra Judicial";
                return "Desconhecido";
            }
        }

        [DisplayName("Probabilidade de Êxito")]
        [Required]
        public string SucceesProbability { get; set; }

        [DisplayName("Probabilidade de Êxito")]
        [NotMapped]
        public string SucceesProbabilityName {
            get {
                if (SucceesProbability == "Possible") return "Possível";
                if (SucceesProbability == "Probable") return "Provável";
                if (SucceesProbability == "Remote") return "Remoto";
                return "Desconhecido";
            }
        }
        #endregion

    }
}
