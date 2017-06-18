using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class AnalysisRequestStatus {

        #region Properties
        
        public string Value { get; set; }
        public string Name { get; set; }

        public bool IsAWorkflowEnd { get; set; }
        
        #endregion

        #region Status

        public static AnalysisRequestStatus WaitingForDocuments {
            get {
                return new AnalysisRequestStatus("WaitingForDocuments", "Aguardando documentos", false);
            }
        }

        public static AnalysisRequestStatus WaitingForAnalysis {
            get {
                return new AnalysisRequestStatus("WaitingForAnalysis", "Aguardando análise", false);
            }
        }

        public static AnalysisRequestStatus WaitingForFeedback {
            get {
                return new AnalysisRequestStatus("WaitingForFeedback", "Aguardando Parecer", false);
            }
        }


        public static AnalysisRequestStatus Approved {
            get {
                return new AnalysisRequestStatus("Approved", "Aprovada", true);
            }
        }

        public static AnalysisRequestStatus ApprovedWithReservationsLevel1 {
            get {
                return new AnalysisRequestStatus("ApprovedWithReservationsLevel1", "Aprovada com reserva - Nível 1", true);
            }
        }

        public static AnalysisRequestStatus ApprovedWithReservationsLevel2 {
            get {
                return new AnalysisRequestStatus("ApprovedWithReservationsLevel2", "Aprovada com reserva - Nível 2", true);
            }
        }

        public static AnalysisRequestStatus Cancelled {
            get {
                return new AnalysisRequestStatus("Cancelled", "Cancelado", true);
            }
        }

        public static AnalysisRequestStatus Disapproved {
            get {
                return new AnalysisRequestStatus("Disapproved", "Reprovado", true);
            }
        }

        #endregion

        private AnalysisRequestStatus(string value, string name, bool isAWorkFlowEnd) {
            Value = value;
            Name = name;
            IsAWorkflowEnd = isAWorkFlowEnd;
        }

        internal static bool TryConvert(string value, ref AnalysisRequestStatus status) {
            try {
                status = Convert(value);
                return true;

            } catch (ArgumentOutOfRangeException) {
                return false;
            }

        }

        public static AnalysisRequestStatus Convert(string value) {
            if (value == WaitingForDocuments.Value) return WaitingForDocuments;
            if (value == WaitingForAnalysis.Value) return WaitingForAnalysis;
            if (value == WaitingForFeedback.Value) return WaitingForFeedback;
            if (value == Approved.Value) return Approved;
            if (value == ApprovedWithReservationsLevel1.Value) return ApprovedWithReservationsLevel1;
            if (value == ApprovedWithReservationsLevel2.Value) return ApprovedWithReservationsLevel2;
            if (value == Disapproved.Value) return Disapproved;
            if (value == Cancelled.Value) return Cancelled;

            throw new ArgumentOutOfRangeException("Status unknown");
        }

        public override bool Equals(object obj) {
            if (obj is AnalysisRequestStatus)
                return Value == ((AnalysisRequestStatus)obj).Value;
            return false;
        }

    }
}
