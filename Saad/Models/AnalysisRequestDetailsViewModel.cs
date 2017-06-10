using Saad.Lib.Data.Helpers;
using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Saad.Models {
    public class AnalysisRequestDetailsViewModel {

        public AnalysisRequest Request { get; set; }
        public DocumentKit Kit { get; set; }

        public IEnumerable<Document> DocumentsToSend {
            get {
                if (Request.Status.Equals(AnalysisRequestStatus.WaitingForDocuments)) {
                    return (from doc in Kit.Documents
                            where Request.Documents.Count(d => d.DocumentReference == doc.Reference) == 0
                            select doc).ToList();
                }
                return new List<Document>();
            }
        }

        public bool CanSubmitEvaluation(System.Security.Principal.IPrincipal User, AnalysisRequestDocument doc) {
            if (Request.Status.IsAWorkflowEnd || Request.Status.Equals(AnalysisRequestStatus.WaitingForFeedback))
                return false;

            if (doc.DocumentReference.EndsWith("ANALISE_PARECER_AREA_COMERCIAL") || doc.DocumentReference.EndsWith("DOCUMENTACAO_TECNICA")
                    || doc.DocumentReference.EndsWith("HOMOLON")) {

                if (User.IsInRole(ApplicationUserRoles.Customer)) {
                    return true;
                }

            }

            if (Request.Type == AnalysisRequest.Litigation) //litigation requests does not have document evaluation
                return false;   

            return User.IsInRole(ApplicationUserRoles.Admin);

        }

    }
}