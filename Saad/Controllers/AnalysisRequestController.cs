using Saad.Helpers;
using Saad.Lib.Data.Model;
using Saad.Lib.Service;
using Saad.Lib.Utils;
using Saad.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Saad.Lib.Data.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Globalization;

namespace Saad.Controllers {
    [Authorize]
    public class AnalysisRequestController : BaseController {

        public AnalysisRequestService AnalysisRequestService { get; set; }
        public UserService UserService { get; set; }
        public DocumentKitService DocumentKitService { get; set; }

        public HelperService HelperService { get; set; }

        public AnalysisRequestController() {
            AnalysisRequestService = new AnalysisRequestService();
            UserService = new UserService();
            DocumentKitService = new DocumentKitService();
            HelperService = new HelperService();
        }

        // GET: AnalysisRequest
        public ActionResult Index(string q, string status, string type, string service, string referenceDate, string endReferenceDate
                                    , string work, string workDescription, int page = 1) {

            ViewBag.CurrentSearch = q;
            ViewBag.CurrentSearchStatus = status;
            ViewBag.CurrentSearchType = type;
            ViewBag.CurrentSearchService = service;
            ViewBag.CurrentSearchReferenceDate = referenceDate;
            ViewBag.CurrentSearchEndReferenceDate = endReferenceDate;
            ViewBag.CurrentSearchWork = work;
            ViewBag.CurrentSearchWorkDescription = workDescription;

            return View(AnalysisRequestService.Search(q, status, type, service, referenceDate, endReferenceDate, work, workDescription, LoggedUser).ToPagedList(page, 10));
        }

        [Authorize(Roles = "Customer, Admin")]
        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RequestAnalysisViewModel model) {
            bool isCnpjValido = Validation.ValidaCnpj(model.Request.Supplier.CNPJ);

            if (!ModelState.IsValid || !isCnpjValido) {
                if (!isCnpjValido)
                    ModelState.AddModelError("", "CNPJ Inválido");

                return View(model);
            }

            #region Save Contract

            if (model.Contract != null && model.Contract.ContentLength > 0) {
                var uploadPath = Server.MapPath("~/Content/Uploads");
                string fileName = Path.GetFileName(string.Concat(Guid.NewGuid() + model.Contract.FileName));
                string filePath = Path.Combine(@uploadPath, fileName);

                model.Contract.SaveAs(filePath);

                model.Request.ContractFileName = model.Contract.FileName;
                model.Request.ContractFileUrl = "/Content/Uploads/" + fileName;

            }

            #endregion

            foreach (var id in model.WorkDescriptions) {
                model.Request.WorkDescriptions.Add(new AnalysisRequestWorkDescriptions() { Id = id });
            }

            AnalysisRequestService.CreateRequest(model.Request, LoggedUser);

            await CreateSupplierLoginAndSendEmail(model.Request);
            await SendNotificationIfCustomerRegistered(model.Request);

            AddMessage(MessageType.Success, "Análise requisitada com sucesso!");

            if (model.CreateAnother)
                return RedirectToAction("Create");

            return RedirectToAction("Index");

        }

        private async Task<bool> CreateSupplierLoginAndSendEmail(AnalysisRequest request) {
            var existsUser = await UserManager.FindByEmailAsync(request.Supplier.MainContactEmail);
            if (existsUser != null) return true;

            var user = new ApplicationUser {
                UserName = request.Supplier.MainContactEmail,
                Email = request.Supplier.MainContactEmail,
                Name = request.Supplier.MainContactName,
                EmailConfirmed = true
            };

            var password = request.Supplier.CNPJ.Replace(".", "").Replace("/", "").Replace("-", "");
            var result = await UserManager.CreateAsync(user, password);

            if (result.Succeeded) {

                var newUser = await UserManager.FindByEmailAsync(request.Supplier.MainContactEmail);
                await UserManager.AddToRoleAsync(newUser.Id, ApplicationUserRoles.Supplier);

                var mail = new MailMessage();
                var client = new SmtpClient();
                mail.IsBodyHtml = true;

                mail.To.Add(new MailAddress(request.Supplier.MainContactEmail));
                mail.Subject = "Dados para Acesso ao SAAD Consult";
                mail.Body = string.Format("<html><body><p>Dados de acesso</p><p>Login: {0}</p><p>Senha: {1}</p></body></html>", request.Supplier.MainContactEmail, password);
                client.Send(mail);

            }

            return true;

        }

        private async Task<bool> SendNotificationIfCustomerRegistered(AnalysisRequest request) {
            var isLoggedUserACustomer = await UserManager.IsInRoleAsync(LoggedUser.Id, ApplicationUserRoles.Customer);
            if (isLoggedUserACustomer) {
                SendNotification(request);
            }

            return true;
        }

        private void SendNotification(AnalysisRequest request) {

            var client = new SmtpClient();
            var mail = new MailMessage();

            mail.IsBodyHtml = true;

            foreach (var email in ApplicationHelper.NotifyHomologationRequestToEmails) {
                mail.To.Add(new MailAddress(email));
            }

            mail.Subject = "Nova Requisição de Homologação";
            mail.Body = string.Format("<html><body><p>Nova Requisição de Homologação</p><p>CNPJ: {0}</p><p>Razão Social: {1}</p></body></html>", request.Supplier.CNPJ, request.Supplier.Name);
            client.Send(mail);
        }

        public ActionResult SendDocuments() {
            return View();
        }

        public ActionResult Details(int id) {
            var request = AnalysisRequestService.Get(id);
            if (request != null) {
                var model = new AnalysisRequestDetailsViewModel();
                model.Request = request;
                model.Kit = DocumentKitService.GetByCode(request.Type);

                return View(model);

            }

            AddMessage(MessageType.Info, "Homologação não encontrada");
            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryToken]
        public ActionResult Upload(int id) {

            int arquivosSalvos = 0;

            for (int i = 0; i < Request.Files.Count; i++) {
                HttpPostedFileBase arquivo = Request.Files[i];

                //Salva o arquivo
                if (arquivo.ContentLength > 0) {
                    var uploadPath = Server.MapPath("~/Content/Uploads");
                    string caminhoArquivo = Path.Combine(@uploadPath, Path.GetFileName(arquivo.FileName));
                    arquivo.SaveAs(caminhoArquivo);

                    var doc = DocumentKitService.GetDocument(Convert.ToInt32(Request.Files.AllKeys[i]));
                    var referenceDate = Request.Form["referencedate-" + doc.Id];

                    if (string.IsNullOrWhiteSpace(referenceDate)) {
                        AddMessage(MessageType.Danger, "Informe a data de referência para o documento: " + doc.Name);
                        continue;
                    }

                    AnalysisRequestService.AddDocumentToAnalysisRequest(id, new AnalysisRequestDocument() {
                        FileName = arquivo.FileName,
                        Url = "/Content/Uploads/" + Path.GetFileName(arquivo.FileName),
                        Name = doc.Name,
                        Type = doc.Type,
                        Relevance = doc.Relevance,
                        MaximumEvaluationPoints = doc.MaximumScore,
                        ReferenceMonth = Convert.ToInt32(referenceDate.Split('/').First()),
                        ReferenceYear = Convert.ToInt32(referenceDate.Split('/').Last()),
                        DocumentReference = doc.Reference
                    }, LoggedUser);

                    arquivosSalvos++;
                }

            }

            var reasons = Request.Form.AllKeys.Where(k => k.StartsWith("reason-"));
            foreach (var reason in reasons) {
                if (!string.IsNullOrWhiteSpace(Request.Form[reason])) {
                    int docId = Convert.ToInt32(reason.Split('-').Last());
                    var doc = DocumentKitService.GetDocument(docId);

                    AnalysisRequestService.AddDocumentToAnalysisRequest(id, new AnalysisRequestDocument() {
                        Name = doc.Name,
                        Type = doc.Type,
                        Relevance = doc.Relevance,
                        MaximumEvaluationPoints = doc.MaximumScore,
                        DocumentReference = doc.Reference,
                        ReasonNotPresent = Request.Form[reason]
                    }, LoggedUser);
                }
            }

            var request = AnalysisRequestService.Get(id);
            var kit = DocumentKitService.GetByCode(request.Type);

            try {
                AnalysisRequestService.TryToMoveForwardOnWorkflow(id, LoggedUser);
                AddMessage(MessageType.Success, "Seus documentos foram enviados para análise");
            } catch (ApplicationException ex) {
                AddMessage(MessageType.Danger, ex.Message);
            }

            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult AddLitigationData(int requestId, AnalysisRequestLitigation data) {
            AnalysisRequestService.AddLitigation(requestId, data);

            AnalysisRequestService.TryToMoveForwardOnWorkflow(requestId, LoggedUser);
            AddMessage(MessageType.Success, "A análise de Contencioso foi finalizada!");

            return RedirectToAction("Details", new { id = requestId });
        }

        public ActionResult Analysis(int id, FormCollection form) {

            var evaluations = form.AllKeys.Where(r => r.StartsWith("evaluation-"));
            bool hasMessage = false;

            foreach (var evaluation in evaluations) {
                int docId = Convert.ToInt32(evaluation.Split('-').Last());

                var reason = form["reason-" + docId];

                int evaluationData = 0;

                if (string.IsNullOrWhiteSpace(form[evaluation]))
                    continue;

                if (!int.TryParse(form[evaluation], out evaluationData)) {
                    if (!hasMessage) {
                        AddMessage(MessageType.Danger, "Nota inválida. Deve ser um valor numérico entre 0 e 10!");
                        hasMessage = true;
                    }
                    continue;
                }

                AnalysisRequestService.EvaluateDocument(id, docId, evaluationData, reason);

            }

            try {
                AnalysisRequestService.TryToMoveForwardOnWorkflow(id, LoggedUser);
            } catch (Exception ex) {
                AddMessage(MessageType.Danger, ex.Message);
            }

            return RedirectToAction("Details", new { id = id });

        }

        [ValidateAntiForgeryToken]
        public ActionResult Feedback(int id, FormCollection form) {

            var feedbacks = form.AllKeys.Where(r => r.StartsWith("type|"));

            foreach (var feedback in feedbacks) {
                var type = feedback.Split('|').Last();

                AnalysisRequestService.InsertFeedback(id, type, form[feedback], LoggedUser);

            }

            try {
                AnalysisRequestService.TryToMoveForwardOnWorkflow(id, LoggedUser);
            } catch (Exception ex) {
                AddMessage(MessageType.Danger, ex.Message);
            }

            return RedirectToAction("Details", new { id = id });

        }

        public ActionResult FinalReport(int id) {
            ViewBag.StandardReasons = DocumentKitService.DocumentStandardList();

            var request = AnalysisRequestService.Get(id);
            return View(new FinalReportViewModel(request, AnalysisRequestService.GetHistory(request.Supplier.CNPJ)));
        }

        public ActionResult Reopen(int id) {
            try {
                AnalysisRequestService.Reopen(id);

            } catch (Exception) {
                Response.StatusCode = 500;

            }

            return Content("");

        }

        public ActionResult GetLastRequest(string cnpj) {
            var request = AnalysisRequestService.GetHistory(cnpj, 1);

            if (request != null && request.Count > 0)
                return Json(ConvertToJson(request.First()), JsonRequestBehavior.AllowGet);

            return HttpNotFound();

        }

        private object ConvertToJson(AnalysisRequest request) {
            return new {
                Id = request.Id,
                Status = request.Status.Name,
                Date = request.CreateDate.ToShortDateString(),
                Type = request.TypeName,
                MonthToDate = Convert.ToInt32(DateTime.Now.Subtract(request.CreateDate).TotalDays / 30),
                Supplier = new {
                    Name = request.Supplier.Name,
                    FantasyName = request.Supplier.FantasyName,
                    Address = new {
                        Zipcode = request.Supplier.CEP,
                        Street = request.Supplier.Street,
                        Number = request.Supplier.Number,
                        District = request.Supplier.District,
                        City = request.Supplier.City,
                        UF = request.Supplier.UF
                    }
                }
            };

        }


        public ActionResult DeleteDocument(int requestId, int documentId) {
            try {
                AnalysisRequestService.DeleteDocument(requestId, documentId);

            } catch (Exception) {
                Response.StatusCode = 500;

            }

            return Content("");
        }

        public ActionResult GenerateLitigationReport() {

            using (ExcelPackage p = new ExcelPackage()) {
                //Here setting some document properties
                
                //Create a sheet
                p.Workbook.Worksheets.Add("Críticos e instância judicial");
                ExcelWorksheet ws = p.Workbook.Worksheets[1];
                ws.Name = "Críticos e instância judicial"; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                var requests = AnalysisRequestService.Search(null, AnalysisRequestStatus.Approved.Value, AnalysisRequest.Litigation, null, null, null, null, null, LoggedUser);

                var rowIndex = 1;

                #region header

                ws.Cells[rowIndex, 1].Value = "CNPJ";
                ws.Cells[rowIndex, 2].Value = "Fornecedor";
                ws.Cells[rowIndex, 3].Value = "C. Custo";
                ws.Cells[rowIndex, 4].Value = "Nome do Empreendimento";
                ws.Cells[rowIndex, 5].Value = "Descrição dos Serviços";
                ws.Cells[rowIndex, 6].Value = "Início";
                ws.Cells[rowIndex, 7].Value = "Término";
                ws.Cells[rowIndex, 8].Value = "Verba (V1)";
                ws.Cells[rowIndex, 9].Value = "Valor da Contratação Incial";
                ws.Cells[rowIndex, 10].Value = "Resultado (Verba x Contratação)";
                ws.Cells[rowIndex, 11].Value = "Aditivos";
                ws.Cells[rowIndex, 12].Value = "Valor Final do Contrato";
                ws.Cells[rowIndex, 13].Value = "Pedidos";
                ws.Cells[rowIndex, 14].Value = "Fornecido";
                ws.Cells[rowIndex, 15].Value = "Saldo bloqueado";
                ws.Cells[rowIndex, 16].Value = "Caução retida";
                ws.Cells[rowIndex, 17].Value = "Adiantamento";
                ws.Cells[rowIndex, 18].Value = "Funcionários";
                ws.Cells[rowIndex, 19].Value = "Impostos";
                ws.Cells[rowIndex, 20].Value = "Fornecedores";
                ws.Cells[rowIndex, 21].Value = "Nova Contratação";
                ws.Cells[rowIndex, 22].Value = "Substituição";
                ws.Cells[rowIndex, 23].Value = "Saldo (Inicial x Final)";
                ws.Cells[rowIndex, 24].Value = "Plano de ação Alphaville";
                ws.Cells[rowIndex, 25].Value = "Processo";
                ws.Cells[rowIndex, 26].Value = "Vara";
                ws.Cells[rowIndex, 27].Value = "Comarca";
                ws.Cells[rowIndex, 28].Value = "Andamento";
                ws.Cells[rowIndex, 29].Value = "Pendências para ingresso da ação";
                ws.Cells[rowIndex, 30].Value = "Tipo";
                ws.Cells[rowIndex, 31].Value = "Probabilidade de Sucesso";

                for (int i = 1; i < 32; i++) {
                    ws.Cells[1, i].Style.Font.Bold = true;
                    ws.Cells[1, i].Style.Font.Color.SetColor(Color.White);

                    ws.Cells[1, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));

                    ws.Cells[1, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    ws.Cells[1, i].Style.Border.Bottom.Color.SetColor(Color.FromArgb(87, 135, 192));
                }
                
                #endregion

                rowIndex++;

                #region first plan
                
                foreach (var request in requests) {
                    if (request.LitigationData == null) continue;

                    ws.Cells[rowIndex, 1].Value = request.Supplier.CNPJ;
                    ws.Cells[rowIndex, 2].Value = request.Supplier.Name;
                    ws.Cells[rowIndex, 3].Value = request.Work.Code;
                    ws.Cells[rowIndex, 4].Value = request.Work.Name;
                    ws.Cells[rowIndex, 5].Value = string.Join(", ", request.WorkDescriptions.Select(d => d.WorkDescription.Name));

                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                    if (request.ContractStartDate.HasValue)
                        ws.Cells[rowIndex, 6].Value = request.ContractStartDate.Value;

                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                    if (request.ContractFinalDate.HasValue)
                        ws.Cells[rowIndex, 7].Value = request.ContractFinalDate.Value;

                    ws.Cells[rowIndex, 8].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 8].Value = request.LitigationData.ContractSum;

                    ws.Cells[rowIndex, 9].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 9].Value = request.LitigationData.InitialContractValue;

                    ws.Cells[rowIndex, 10].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 10].Value = request.LitigationData.Result;

                    ws.Cells[rowIndex, 11].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 11].Value = request.LitigationData.ContractAdditionsValue;

                    ws.Cells[rowIndex, 12].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 12].Value = request.LitigationData.ContractTotal;

                    ws.Cells[rowIndex, 13].Value = request.LitigationData.Orders;

                    ws.Cells[rowIndex, 14].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 14].Value = request.LitigationData.ContractSuppliedValue;
                    
                    ws.Cells[rowIndex, 15].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 15].Value = request.LitigationData.ContractBlockedValue;

                    ws.Cells[rowIndex, 16].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 16].Value = request.LitigationData.BondValue;

                    ws.Cells[rowIndex, 17].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 17].Value = request.LitigationData.ContractAdvanceValue;

                    ws.Cells[rowIndex, 18].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 18].Value = request.LitigationData.ContractEmployeesPayment;

                    ws.Cells[rowIndex, 19].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 19].Value = request.LitigationData.TaxesPaymentValue;

                    ws.Cells[rowIndex, 20].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 20].Value = request.LitigationData.SuppliersPaymentValue;

                    ws.Cells[rowIndex, 21].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 21].Value = request.LitigationData.HiringValue;

                    ws.Cells[rowIndex, 22].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 22].Value = request.LitigationData.ReplacementTotal;

                    ws.Cells[rowIndex, 23].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 23].Value = request.LitigationData.Balance;

                    ws.Cells[rowIndex, 24].Value = request.LitigationData.ActionPlan;
                    ws.Cells[rowIndex, 25].Value = request.LitigationData.ProcessCode;
                    ws.Cells[rowIndex, 26].Value = request.LitigationData.JudgmentPlace;
                    ws.Cells[rowIndex, 27].Value = request.LitigationData.JudicialDistrict;
                    ws.Cells[rowIndex, 28].Value = request.LitigationData.ProgressDescription;
                    ws.Cells[rowIndex, 29].Value = request.LitigationData.PendenciesBlockingIngress;
                    ws.Cells[rowIndex, 30].Value = request.LitigationData.LitigationTypeName;
                    ws.Cells[rowIndex, 31].Value = request.LitigationData.SucceesProbabilityName;

                    var color = Color.FromArgb(255, 255, 255);
                    if (rowIndex % 2 == 1)
                        color = Color.FromArgb(220, 230, 241);

                    for (int i = 1; i < 32; i++) {
                        ws.Cells[rowIndex, i].Style.Font.Bold = true;
                        ws.Cells[rowIndex, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[rowIndex, i].Style.Fill.BackgroundColor.SetColor(color);
                        
                        ws.Cells[rowIndex, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        ws.Cells[rowIndex, i].Style.Border.Bottom.Color.SetColor(Color.FromArgb(210, 223, 237));
                    }
                
                    rowIndex++;

                }

                ws.Cells["A1:AE" + rowIndex].AutoFitColumns();

                #endregion

                #region second plan

                p.Workbook.Worksheets.Add("Resumo");
                ws = p.Workbook.Worksheets[2];
                ws.Name = "Resumo"; //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                var data = (from request in requests
                            where request.LitigationData != null
                            group request by request.Supplier.Name into g
                            select new {
                                SupplierName = g.Key,
                                ContractTotal = g.Sum(r => r.LitigationData.ContractTotal),
                                SuppliedTotal = g.Sum(r => r.LitigationData.ContractSuppliedValue),
                                BlockedTotal = g.Sum(r => r.LitigationData.ContractBlockedValue),
                                ReplacementTotal = g.Sum(r => r.LitigationData.ReplacementTotal),
                                BondTotal = g.Sum(r => r.LitigationData.BondValue),
                            });


                rowIndex = 1;

                #region header

                ws.Cells[rowIndex, 1].Value = "FORNECEDOR";
                ws.Cells[rowIndex, 2].Value = "VALOR FINAL DO CONTRATO";
                ws.Cells[rowIndex, 3].Value = "FORNECIDO";
                ws.Cells[rowIndex, 4].Value = "SALDO BLOQUEADO";
                ws.Cells[rowIndex, 5].Value = "CUSTO DE SUBSTITUIÇÃO";
                ws.Cells[rowIndex, 6].Value = "Soma de CAUÇÃO RETIDA";
                ws.Cells[rowIndex, 7].Value = "SALDO (INICIAL X FINAL)";
                
                for (int i = 1; i < 8; i++) {
                    ws.Cells[1, i].Style.Font.Bold = true;
                    ws.Cells[1, i].Style.Font.Color.SetColor(Color.White);

                    ws.Cells[1, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));

                    ws.Cells[1, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    ws.Cells[1, i].Style.Border.Bottom.Color.SetColor(Color.FromArgb(87, 135, 192));
                }

                #endregion

                rowIndex++;

                foreach (var entry in data) {
                    ws.Cells[rowIndex, 1].Value = entry.SupplierName;

                    ws.Cells[rowIndex, 2].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 2].Value = entry.ContractTotal;

                    ws.Cells[rowIndex, 3].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 3].Value = entry.SuppliedTotal;

                    ws.Cells[rowIndex, 4].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 4].Value = entry.BlockedTotal;

                    ws.Cells[rowIndex, 5].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 5].Value = entry.ReplacementTotal;

                    ws.Cells[rowIndex, 6].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 6].Value = entry.ReplacementTotal;
                    
                    ws.Cells[rowIndex, 7].Style.Numberformat.Format = "R$ #,##0.00";
                    ws.Cells[rowIndex, 7].Value = entry.BondTotal;

                    var color = Color.FromArgb(255, 255, 255);
                    if (rowIndex % 2 == 1)
                        color = Color.FromArgb(220, 230, 241);

                    for (int i = 1; i < 8; i++) {
                        ws.Cells[rowIndex, i].Style.Font.Bold = true;
                        ws.Cells[rowIndex, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[rowIndex, i].Style.Fill.BackgroundColor.SetColor(color);

                        ws.Cells[rowIndex, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                        ws.Cells[rowIndex, i].Style.Border.Bottom.Color.SetColor(Color.FromArgb(210, 223, 237));
                    }

                    rowIndex++;

                }

                ws.Cells[rowIndex, 1].Value = "Total";

                ws.Cells[rowIndex, 2].Style.Numberformat.Format = "R$ #,##0.00";
                ws.Cells[rowIndex, 2].Value = data.Sum(e => e.ContractTotal);

                ws.Cells[rowIndex, 3].Style.Numberformat.Format = "R$ #,##0.00";
                ws.Cells[rowIndex, 3].Value = data.Sum(e => e.SuppliedTotal);

                ws.Cells[rowIndex, 4].Style.Numberformat.Format = "R$ #,##0.00";
                ws.Cells[rowIndex, 4].Value = data.Sum(e => e.BlockedTotal);

                ws.Cells[rowIndex, 5].Style.Numberformat.Format = "R$ #,##0.00";
                ws.Cells[rowIndex, 5].Value = data.Sum(e => e.ReplacementTotal);

                ws.Cells[rowIndex, 6].Style.Numberformat.Format = "R$ #,##0.00";
                ws.Cells[rowIndex, 6].Value = data.Sum(e => e.ReplacementTotal);

                ws.Cells[rowIndex, 7].Style.Numberformat.Format = "R$ #,##0.00";
                ws.Cells[rowIndex, 7].Value = data.Sum(e => e.BondTotal);

                ws.Cells["A1:AC" + rowIndex].AutoFitColumns();

                #endregion

                Byte[] bin = p.GetAsByteArray();

                return File(bin, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            }

            

        }
    }
}