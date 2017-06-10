using log4net;
using Saad.Lib.Data.EntityFramework;
using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Service {
    public class AnalysisRequestMonthlyControlService {
        
        private Context context;

        public AnalysisRequestMonthlyControlService() {
            context = new Context();
        }

        public void GenerateMonthlyControl(ILog log) {
            var now = DateTime.Now;

            var control = GetOrCreate(now.Month, now.Year);
            var finalizedHomologations = from request in context.AnalysisRequest
                                       where request.Type == AnalysisRequest.Homologation 
                                                && (request.RequestStatus == AnalysisRequestStatus.Approved.Value
                                                    || request.RequestStatus == AnalysisRequestStatus.ApprovedWithReservationsLevel1.Value
                                                    || request.RequestStatus == AnalysisRequestStatus.ApprovedWithReservationsLevel2.Value)
                                       select request;

            foreach (var homologation in finalizedHomologations.ToList()) {
                try {
                    if (ShouldCreateAnMonthlyAnalysisRequest(homologation, now)) {

                        context.AnalysisRequest.Add(CreateRequest(homologation));
                        context.SaveChanges();

                        SendCreateMonthlyAnalysisNotification(homologation);

                    }

                } catch (Exception ex) {
                    log.Error(ex);
                }

            }

        }

        private bool ShouldCreateAnMonthlyAnalysisRequest(AnalysisRequest homologation, DateTime refDate) {

            bool hasMonthlyRequestThisMount = (context.AnalysisRequest.Count(r => r.CreateDate.Month == refDate.Month
                                                        && r.CreateDate.Year == refDate.Year
                                                        && r.Supplier.Id == homologation.Supplier.Id
                                                        && r.Type == AnalysisRequest.MonthlyAnalysis) > 0);

            bool hasHomologationHappenedThisMount = (homologation.CreateDate.Month == refDate.Month && homologation.CreateDate.Year == refDate.Year);

            return (!hasMonthlyRequestThisMount && !hasHomologationHappenedThisMount);

        }

        private AnalysisRequest CreateRequest(AnalysisRequest homologation) {
            var request = new AnalysisRequest() {
                Type = AnalysisRequest.MonthlyAnalysis,
                Supplier = homologation.Supplier,
                CreateDate = DateTime.Now,
                ServiceLocal = homologation.ServiceLocal,
                ServiceDescription = homologation.ServiceDescription,
                Customer = homologation.Customer,
                RequestedByUser = homologation.RequestedByUser,
                Status = AnalysisRequestStatus.WaitingForDocuments

            };

            request.Activities.Add(new AnalysisRequestActivity {
                Date = DateTime.Now,
                Status = "Requisição",
                Title = "Análise Mensal",
                Remarks = string.Format("{0} requisitou a análise mensal", "Sistema"),

            });

            return request;

        }

        private void SendCreateMonthlyAnalysisNotification(AnalysisRequest request) {
            using (var client = new SmtpClient()) {
                var mail = new MailMessage();

                mail.IsBodyHtml = true;
                mail.To.Add(new MailAddress(request.Supplier.MainContactEmail));

                mail.Subject = string.Format("Requisição de Análise Mensal");
                mail.Body = string.Format("<html><body><p>A SAAD Consult solicita o envio dos documentos para acompanhamento mensal de suas atividades</p><p>CNPJ: {0}</p><p>Razão Social: {1}</p></body></html>", request.Supplier.CNPJ, request.Supplier.Name);
                client.Send(mail);

            }
        }

        private AnalysisRequestMonthlyControl GetOrCreate(int month, int year) {
            var control = Get(month, year);
            if (control == null)
                control = Create(month, year);

            return control;

        }

        public AnalysisRequestMonthlyControl Get(int month, int year) {
            return context.AnalysisRequestMonthlyControls.SingleOrDefault(c => c.CreateDate.Month == month && c.CreateDate.Year == year);
        }

        private AnalysisRequestMonthlyControl Create(int month, int year) {
            var control = new AnalysisRequestMonthlyControl() {
                CreateDate = DateTime.Now,
                ErrorsCount = 0,
                TotalRequestsGenerated = 0
            };
            context.AnalysisRequestMonthlyControls.Add(control);
            context.SaveChanges();
            
            return control;

        }

    }
}
