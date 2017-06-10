using log4net;
using Saad.Lib.Data.EntityFramework;
using Saad.Lib.Data.Helpers;
using Saad.Lib.Data.Model;
using Saad.Lib.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Service {
    public class AnalysisRequestService {

        private Context context;

        public AnalysisRequestService() {
            context = new Context();
        }

        public void CreateRequest(AnalysisRequest request, ApplicationUser loggedUser) {

            var requestedBy = GetRequestedByUser(request, loggedUser);
            request.RequestedByUser = requestedBy;
            request.Customer = context.Customers.Single(c => c.Id == requestedBy.Customer.Id);
            request.CreateDate = DateTime.Now;

            //resolving relationships
            request.Work = context.Works.SingleOrDefault(w => w.Id == request.Work.Id);
            for (int i = 0; i < request.WorkDescriptions.Count; i++) {
                int workDescriptionId = request.WorkDescriptions[i].Id;
                request.WorkDescriptions[i].WorkDescription = context.WorkDescriptions.SingleOrDefault(w => w.Id == workDescriptionId);
                request.WorkDescriptions[i].Request = request;

            }

            if (request.RequestedByDepartment != null && request.RequestedByDepartment.Id > 0)
                request.RequestedByDepartment = context.Departments.SingleOrDefault(d => d.Id == request.RequestedByDepartment.Id);

            context.AnalysisRequest.Add(request);

            try {
                context.SaveChanges();

                AddActivityToAnalysisRequest(request, new AnalysisRequestActivity {
                    Status = "Requisição",
                    Title = request.TypeName,
                    Remarks = string.Format("{0} requisitou a {1}", loggedUser.Name, request.TypeName),

                });

            } catch (DbEntityValidationException validation) {
                throw;
            }

        }

        private ApplicationUser GetRequestedByUser(AnalysisRequest request, ApplicationUser loggedUser) {
            var userId = loggedUser.Roles.Count(r => r.RoleId == ApplicationUserRoles.Customer) > 0 ? loggedUser.Id : request.RequestedByUser.Id;

            return context.Users.FirstOrDefault(u => u.Id == userId);

        }

        public IEnumerable<AnalysisRequest> Search(string q, string status, string type, string service, string referenceDate, string endReferenceDate
                                                    , string work, string workDescription, ApplicationUser loggedUser) {

            var query = from request in context.AnalysisRequest
                        select request;

            if (!string.IsNullOrWhiteSpace(q)) {
                if (Validation.ValidaCnpj(q)) {
                    string cnpj = Convert.ToUInt64(q.Replace(".", "").Replace("-", "").Replace("/", "")).ToString(@"00\.000\.000\/0000\-00");
                    query = query.Where(r => r.Supplier.CNPJ == cnpj);
                } else {
                    query = query.Where(r => r.Supplier.FantasyName.Contains(q) || r.Supplier.Name.Contains(q));
                }

            }

            if (!string.IsNullOrWhiteSpace(status)) {
                AnalysisRequestStatus requestStatus = null;
                if (AnalysisRequestStatus.TryConvert(status, ref requestStatus)) {
                    query = query.Where(r => r.RequestStatus == status);
                }
            }

            if (!string.IsNullOrWhiteSpace(type)) {
                query = query.Where(r => r.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(service)) {
                query = query.Where(r => r.ServiceDescription.Contains(service));
            }

            if (!string.IsNullOrWhiteSpace(referenceDate)) {
                var data = referenceDate.Split('/');
                var startDate = new DateTime(Convert.ToInt32(data[2]), Convert.ToInt32(data[1]), Convert.ToInt32(data[0]));
                
                query = query.Where(r => DbFunctions.TruncateTime(r.ReferenceDate) >= startDate);
                
            }

            if (!string.IsNullOrWhiteSpace(endReferenceDate)) {
                var data = endReferenceDate.Split('/');
                var finishDate = new DateTime(Convert.ToInt32(data[2]), Convert.ToInt32(data[1]), Convert.ToInt32(data[0]));

                query = query.Where(r => DbFunctions.TruncateTime(r.ReferenceDate) <= finishDate);
                
            }

            int workId = 0;
            if (!string.IsNullOrEmpty(work) && int.TryParse(work, out workId)) {
                query = query.Where(r => r.Work.Id == workId);
            }

            //int workDescriptionId = 0;
            //TODO - change it
            //if (!string.IsNullOrEmpty(workDescription) && int.TryParse(workDescription, out workDescriptionId)) {
            //    query = query.Where(r => r.WorkDescription.Id == workDescriptionId);
            //}

            #region authorization rules

            if (loggedUser.Roles.Count(r => r.RoleId == ApplicationUserRoles.Customer) > 0) {
                query = query.Where(r => r.Customer.Id == loggedUser.Customer.Id);
            }

            if (loggedUser.Roles.Count(r => r.RoleId == ApplicationUserRoles.Supplier) > 0) {
                query = query.Where(r => r.Supplier.MainContactEmail == loggedUser.Email);
            }

            #endregion

            query = query.OrderByDescending(r => r.CreateDate);

            return query;

        }

        public void AddDocumentToAnalysisRequest(int requestId, AnalysisRequestDocument document, ApplicationUser loggedUser) {
            var request = Get(requestId);
            request.Documents.Add(document);
            document.AnalysisRequest = request;

            if (!request.ReferenceDate.HasValue && document.ReferenceMonth > 0) {
                request.ReferenceDate = new DateTime(document.ReferenceYear, document.ReferenceMonth, 1);
            } 

            context.SaveChanges();

            AddActivityToAnalysisRequest(request, new AnalysisRequestActivity {
                Status = "Upload arquivo",
                Title = document.Name,
                Remarks = string.Format("{0} fez upload do arquivo {1}", loggedUser.Name, document.FileName),
                  
            });

            using (var client = new SmtpClient()) {
                var mail = new MailMessage();

                mail.IsBodyHtml = true;

                mail.To.Add(new MailAddress(request.RequestedByUser.Email));
                mail.To.Add(new MailAddress(ConfigurationManager.AppSettings["SAAD.Notify.HomologationRequest"]));

                var url = ConfigurationManager.AppSettings["Request.Url"];

                mail.Subject = string.Format("Novo Documento - CNPJ: {0} - {1}", request.Supplier.CNPJ, request.Supplier.Name);
                mail.Body = string.Format(@"<html><body><p>O fornecedor inseriu um novo documento</p>
                                            <p>CNPJ: {0}</p>
                                            <p>Razão Social: {1}</p>
                                            <p>Status: {2}</p>
                                            <p>Novo Documento: {3}</p>                                            
                                            <p>Url: {4}/{5}</p>
                                            </body></html>", request.Supplier.CNPJ, request.Supplier.Name, request.Status.Name, document.Name, url, request.Id);
                client.Send(mail);

            }

        }

        public void AddActivityToAnalysisRequest(int requestId, AnalysisRequestActivity activity) {
            var request = Get(requestId);
            request.Activities.Add(activity);

            activity.Date = DateTime.Now;
            activity.AnalysisRequest = request;

            context.SaveChanges();
        }

        public void AddActivityToAnalysisRequest(AnalysisRequest request, AnalysisRequestActivity activity) {
            request.Activities.Add(activity);

            activity.Date = DateTime.Now;
            activity.AnalysisRequest = request;

            context.SaveChanges();
        }

        public AnalysisRequest Get(int id) {
            return context.AnalysisRequest.SingleOrDefault(r => r.Id == id);
        }

        public void TryToMoveForwardOnWorkflow(int id, ApplicationUser loggedUser) {
            var request = Get(id);
            var kit = context.DocumentKits.FirstOrDefault(k => k.Code == request.Type);

            if (request.Status.Equals(AnalysisRequestStatus.WaitingForDocuments)) {
                var documentsCounts = kit.Documents.Count;
                if (request.Customer.Id != 1) documentsCounts--;    //não contar a nota molon
                if (request.Documents.Count >= documentsCounts) {
                    request.Status = AnalysisRequestStatus.WaitingForAnalysis;

                    try {
                        context.SaveChanges();
                    } catch (DbEntityValidationException ex) {
                        throw;
                    }
                    
                    return;
                } else {
                    throw new ApplicationException("Ainda há documentos faltando para completar a homologação");
                }
            }

            if (request.Status.Equals(AnalysisRequestStatus.WaitingForAnalysis)) {
                if (request.Type != AnalysisRequest.Litigation) {
                    if (request.Documents.All(d => d.EvaluationPoints.HasValue)) {
                        request.Status = AnalysisRequestStatus.WaitingForFeedback;
                        context.SaveChanges();
                        return;
                    } else {
                        throw new ApplicationException("Ainda há documentos faltando para completar a homologação");
                    }

                } else {
                    request.Status = AnalysisRequestStatus.Approved;
                    context.SaveChanges();
                    return;

                }
            }

            if (request.Status.Equals(AnalysisRequestStatus.WaitingForFeedback)) {

                var validDocuments = request.Documents.Where(r => !r.DocumentReference.Contains("HOMOLON"));

                var maximumPossibleEvaluation = validDocuments.Sum(r => r.MaximumEvaluationPoints.GetValueOrDefault(10));
                var totalEvaluation = validDocuments.Sum(r => r.EvaluationPoints.GetValueOrDefault(0));

                var evaluation = (decimal)totalEvaluation / (decimal)maximumPossibleEvaluation * 100;

                int disapprovalRate = int.Parse(ConfigurationManager.AppSettings["DisapprovalRate"]);
                int level2ApprovalRate = int.Parse(ConfigurationManager.AppSettings["Level2ApprovalRate"]);
                int level1ApprovalRate = int.Parse(ConfigurationManager.AppSettings["Level1ApprovalRate"]);

                request.EvaluationPercentage = evaluation;
                if (evaluation < disapprovalRate) {
                    request.Status = AnalysisRequestStatus.Disapproved;
                } else if (evaluation < level2ApprovalRate) {
                    request.Status = AnalysisRequestStatus.ApprovedWithReservationsLevel2;
                } else if (evaluation < level1ApprovalRate) {
                    request.Status = AnalysisRequestStatus.ApprovedWithReservationsLevel1;
                } else {
                    request.Status = AnalysisRequestStatus.Approved;
                }

                context.SaveChanges();
                AddActivityToAnalysisRequest(request, new AnalysisRequestActivity() {
                    Status = "Análise",
                    Title = request.TypeName,
                    Remarks = string.Format("{0} finalizou a análise da documentação da requisição", loggedUser.Name)
                });

                SendAnalysisResultEmail(request);

                return;

            }

            throw new ApplicationException("Workflow move unknown or impossible");

        }

        private void SendAnalysisResultEmail(AnalysisRequest request) {
            using (var client = new SmtpClient()) {
                var mail = new MailMessage();

                mail.IsBodyHtml = true;

                mail.To.Add(new MailAddress(request.RequestedByUser.Email));
                mail.To.Add(new MailAddress(request.Supplier.MainContactEmail));

                mail.Subject = string.Format("Requisição de Homologação {0} - CNPJ: {1} - {2}", request.Status.Name, request.Supplier.CNPJ, request.Supplier.Name);
                mail.Body = string.Format("<html><body><p>Segue o status da finalização da análise da Requisição de Homologação</p><p>CNPJ: {0}</p><p>Razão Social: {1}</p><p>Status: {2}</p></body></html>", request.Supplier.CNPJ, request.Supplier.Name, request.Status.Name);
                client.Send(mail);
                
            }
        }

        public void EvaluateDocument(int requestId, int documentId, int evaluationData, string reason) {
            var document = context.AnalysisRequestDocuments.FirstOrDefault(d => d.Id == documentId);
            document.EvaluationPoints = evaluationData;
            document.EvaluationRemarks = reason;

            context.SaveChanges();

        }

        public void SendExpiringPeriodToSendDocumentsNotification(ILog log) {

            var expiringRequests = GetExpiringPeriodToSendDocuments();

            foreach (var expiringRequest in expiringRequests) {
                try {

                    SendExpiringPeriodToSendDocumentsNotification(expiringRequest);
                    

                } catch (Exception ex) {
                    log.Error("Send expiring time to send documents error", ex);
                }

            }

            try {
                SendExpiringPeriodToSendDocumentsToCustomerNotification(expiringRequests);
                SendExpiringPeriodToSendDocumentsToSaadNotification(expiringRequests);

            } catch (Exception ex) {
                log.Error("Send expiring time to send documents error to customer or saad", ex);
            }
            
        }

        public IEnumerable<AnalysisRequest> GetExpiringPeriodToSendDocuments() {
            var list = new List<AnalysisRequest>();

            int homologationExpiringTime = Convert.ToInt32(ConfigurationManager.AppSettings["SAAD.Homologation.ExpiringTimeDocuments.Minutes"]);
            int monthlyExpiringTime = Convert.ToInt32(ConfigurationManager.AppSettings["SAAD.Monthly.ExpiringTimeDocuments.Minutes"]);

            list.AddRange(context.AnalysisRequest.Where(r => r.RequestStatus == AnalysisRequestStatus.WaitingForDocuments.Value
                                                    && r.Type == AnalysisRequest.Homologation
                                                    && DbFunctions.AddMinutes(r.CreateDate, homologationExpiringTime) < DateTime.Now));


            list.AddRange(context.AnalysisRequest.Where(r => r.RequestStatus == AnalysisRequestStatus.WaitingForDocuments.Value
                                                    && r.Type == AnalysisRequest.MonthlyAnalysis
                                                    && DbFunctions.AddMinutes(r.CreateDate, monthlyExpiringTime) < DateTime.Now));

            return list;

        }

        private void SendExpiringPeriodToSendDocumentsNotification(AnalysisRequest expiringRequest) {
            using (var client = new SmtpClient()) {
                var mail = new MailMessage();

                mail.IsBodyHtml = true;

                mail.To.Add(new MailAddress(expiringRequest.Supplier.MainContactEmail));
                
                mail.Subject = string.Format("Tempo para Envio Documentos está expirando");
                mail.Body = string.Format("<html><body><p>Está acabando o tempo para envio dos documentos</p><p>CNPJ: {0}</p><p>Razão Social: {1}</p></body></html>", expiringRequest.Supplier.CNPJ, expiringRequest.Supplier.Name);
                client.Send(mail);

            }
        }

        private void SendExpiringPeriodToSendDocumentsToSaadNotification(IEnumerable<AnalysisRequest> expiringRequests) {
            
            var docs = context.Documents.ToList();

            string body = "<html><body><p>Está acabando o tempo para envio dos documentos das seguintes empresas:</p>{0}</body></html>";
            
            var requestItems = new StringBuilder();
            foreach (var request in expiringRequests) {
                requestItems.AppendFormat("<p>CNPJ: <strong>{0}</strong> - Razão Social: <strong>{1}</strong> - {2}</p>", request.Supplier.CNPJ, request.Supplier.Name, request.TypeName);

                var missingDocs = RetrieveMissingDocs(request, docs);
                if (missingDocs.Count > 0) {
                    requestItems.Append("<p>&nbsp;&nbsp;&nbsp;<strong>Documentos faltando:</strong></p>");
                    foreach (var doc in missingDocs) {
                        requestItems.AppendFormat("<p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}</p>", doc.Name);
                    }
                }

            }


            using (var client = new SmtpClient()) {
                var mail = new MailMessage();

                mail.IsBodyHtml = true;
                mail.To.Add(new MailAddress(ConfigurationManager.AppSettings["SAAD.Notify.HomologationRequest"]));
                mail.Subject = string.Format("Tempo para Envio Documentos está expirando");

                mail.Body = string.Format(body, requestItems.ToString());
                client.Send(mail);

            }

        }

        private void SendExpiringPeriodToSendDocumentsToCustomerNotification(IEnumerable<AnalysisRequest> expiringRequests) {
            var customers = from request in expiringRequests
                            group request by request.Customer.Id into g
                            select new { CustomerId = g.Key, Requests = g.ToList() };

            var docs = context.Documents.ToList();

            foreach (var customer in customers) {

                var body = "<html><body><p>Está acabando o tempo para envio dos documentos das seguintes empresas:</p>{0}</body></html>";

                var requestItems = new StringBuilder();
                foreach (var request in customer.Requests) {
                    requestItems.AppendFormat("<p>CNPJ: <strong>{0}</strong> - Razão Social: <strong>{1}</strong> - {2}</p>", request.Supplier.CNPJ, request.Supplier.Name, request.TypeName);

                    var missingDocs = RetrieveMissingDocs(request, docs);
                    if (missingDocs.Count > 0) {
                        requestItems.Append("<p>&nbsp;&nbsp;&nbsp;<strong>Documentos faltando:</strong></p>");
                        foreach (var doc in missingDocs) {
                            requestItems.AppendFormat("<p>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0}</p>", doc.Name);
                        }
                    }

                }

                using (var client = new SmtpClient()) {
                    var mail = new MailMessage();

                    mail.IsBodyHtml = true;
                    mail.To.Add(new MailAddress(ConfigurationManager.AppSettings["SAAD.Notify.ExpiringPeriod.Customer." + customer.CustomerId]));
                    mail.Subject = string.Format("Tempo para Envio Documentos está expirando");

                    mail.Body = string.Format(body, requestItems.ToString());
                    client.Send(mail);

                }
            }
        }

        private IList<Document> RetrieveMissingDocs(AnalysisRequest request, List<Document> docs) {
            return (from doc in docs
                   where request.Documents.Count(d => d.DocumentReference == doc.Reference) == 0
                   select doc).ToList();
                    
        }

        public void InsertFeedback(int requestId, string documentType, string feedback, ApplicationUser loggedUser) {
            var request = Get(requestId);
            var exists = request.Feedbacks.SingleOrDefault(f => f.Type == documentType);
            if (exists != null) {
                exists.Feedback = feedback;

            } else {
                request.Feedbacks.Add(new AnalysisRequestFeedback {
                    AnalysisRequest = request,
                    CreateDate = DateTime.Now,
                    CreateUser = context.Users.Single(u => u.Id == loggedUser.Id),
                    Feedback = feedback,
                    Type = documentType,
                });

            }

            context.SaveChanges();
        }


        public void Reopen(int id) {
            var request = Get(id);
            if (request.Status.IsAWorkflowEnd) {
                request.Status = AnalysisRequestStatus.WaitingForAnalysis;
                context.SaveChanges();
            } else {
                throw new ApplicationException("Request is not in a status that allows reopening");
            }
        }

        public IList<AnalysisRequest> List(DateTime? startDate, DateTime? finishDate, string SupplierCNPJ, int? workId) {
            var query = from request in context.AnalysisRequest
                        select request;

            if (startDate.HasValue) {
                query = query.Where(r => DbFunctions.TruncateTime(r.ReferenceDate) >= startDate.Value);
            }

            if (finishDate.HasValue) {
                query = query.Where(r => DbFunctions.TruncateTime(r.ReferenceDate) <= finishDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(SupplierCNPJ)) {
                query = query.Where(r => r.Supplier.CNPJ == SupplierCNPJ);
            }

            if (workId.HasValue && workId.GetValueOrDefault() > 0) {
                query = query.Where(r => r.Work.Id == workId);
            }

            return query.ToList();
        }

        public IList<AnalysisRequest> GetHistory(string cnpj, int? limit = 10) {
            return context.AnalysisRequest.Where(r => r.Supplier.CNPJ == cnpj).OrderByDescending(r => r.Id).Take(10).ToList();
        }

        public void DeleteDocument(int requestId, int documentId) {
            var request = Get(requestId);
            if (!(request.Status.Equals(AnalysisRequestStatus.WaitingForDocuments) || request.Status.Equals(AnalysisRequestStatus.WaitingForAnalysis)))
                throw new ApplicationException("Requisição deve estar no status 'Aguardando documentos' ou 'Aguardando análise' para que seja possível remover os seus documentos");

            var doc = request.Documents.SingleOrDefault(d => d.Id == documentId);
            if (doc == null)
                throw new ApplicationException("Documento não pertence a essa requisição ou já foi removido");

            request.Status = AnalysisRequestStatus.WaitingForDocuments;
            context.AnalysisRequestDocuments.Remove(doc);
            context.SaveChanges();

        }

        public void AddLitigation(int requestId, AnalysisRequestLitigation data) {
            var request = Get(requestId);
            if (request.LitigationData != null) {
                context.Litigations.Remove(request.LitigationData);
            }
                   
            request.LitigationData = data;
            data.Request = request;

            try {
                context.SaveChanges();
            } catch (DbEntityValidationException ex) {
                throw;
            }
            

        }
    }
}
