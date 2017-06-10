using Saad.Lib.Data.Model;
using Saad.Lib.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Saad {
    public static class HMTLHelperExtensions {

        public static HelperService HelperService = new HelperService();

        public static string IsSelected(this HtmlHelper html, string controller = null, string action = null, string cssClass = null) {

            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            if (String.IsNullOrEmpty(controller))
                controller = currentController;

            if (String.IsNullOrEmpty(action))
                action = currentAction;

            return controller == currentController && action == currentAction ?
                cssClass : String.Empty;
        }

        public static string PageClass(this HtmlHelper html) {
            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            return currentAction;
        }

        public static IEnumerable<SelectListItem> GetUFList(this HtmlHelper html) {
            yield return new SelectListItem() { Value = "AC", Text = "Acre" };
            yield return new SelectListItem() { Value = "AL", Text = "Alagoas" };
            yield return new SelectListItem() { Value = "AP", Text = "Amapá" };
            yield return new SelectListItem() { Value = "AM", Text = "Amazonas" };
            yield return new SelectListItem() { Value = "BA", Text = "Bahia" };
            yield return new SelectListItem() { Value = "CE", Text = "Ceará" };
            yield return new SelectListItem() { Value = "DF", Text = "Distrito Federal" };
            yield return new SelectListItem() { Value = "ES", Text = "Espírito Santo" };
            yield return new SelectListItem() { Value = "GO", Text = "Goiás" };
            yield return new SelectListItem() { Value = "MA", Text = "Maranhão" };
            yield return new SelectListItem() { Value = "MT", Text = "Mato Grosso" };
            yield return new SelectListItem() { Value = "MS", Text = "Mato Grosso do Sul" };
            yield return new SelectListItem() { Value = "MG", Text = "Minas Gerais" };
            yield return new SelectListItem() { Value = "PA", Text = "Pará" };
            yield return new SelectListItem() { Value = "PB", Text = "Paraíba" };
            yield return new SelectListItem() { Value = "PR", Text = "Paraná" };
            yield return new SelectListItem() { Value = "PE", Text = "Pernambuco" };
            yield return new SelectListItem() { Value = "PI", Text = "Piauí" };
            yield return new SelectListItem() { Value = "RJ", Text = "Rio de Janeiro" };
            yield return new SelectListItem() { Value = "RN", Text = "Rio Grande do Norte" };
            yield return new SelectListItem() { Value = "RS", Text = "Rio Grande do Sul" };
            yield return new SelectListItem() { Value = "RO", Text = "Rondônia" };
            yield return new SelectListItem() { Value = "RR", Text = "Roraima" };
            yield return new SelectListItem() { Value = "SC", Text = "Santa Catarina" };
            yield return new SelectListItem() { Value = "SP", Text = "São Paulo" };
            yield return new SelectListItem() { Value = "SE", Text = "Sergipe" };
            yield return new SelectListItem() { Value = "TO", Text = "Tocantins" };

        }

        public static IEnumerable<SelectListItem> GetCustomerUsers(this HtmlHelper html) {
            return from user in new UserService().ListCustomerUsers()
                   select new SelectListItem() {
                       Value = user.Id,
                       Text = user.Name
                   };
        }

        public static IEnumerable<SelectListItem> GetCustomersList(this HtmlHelper html) {
            return from customer in new CustomerService().List()
                   select new SelectListItem() {
                       Value = customer.Id.ToString(),
                       Text = customer.Name
                   };
        }

        public static string TranslateDocumentEvaluationStatus(this HtmlHelper html, string value) {
            switch (value) {
                case "approve":
                    return "Aprovado";
                case "approveWithReservations":
                    return "Aprovado com ressalvas";
                case "disapprove":
                    return "Reprovado";

            }
            return string.Empty;

        }

        public static string ConvertAnalysisRequestStatusToLabelCss(this HtmlHelper html, string status) {
            switch (status) {
                case "WaitingForDocuments": return "label-warning";
                case "WaitingForAnalysis": return "label-primary";
                case "Approved": return "label-success";
                case "ApprovedWithReservationsLevel1": return "label-warning";
                case "ApprovedWithReservationsLevel2": return "label-warning";
                case "Disapproved": return "label-danger";

                default: return "label-primary";
            }
        }

        public static IHtmlString EllipsisElementWithTooltip(this HtmlHelper html, string element, string text, int totalLength) {
            bool needsTooltip = (text.Length > totalLength);

            string value = text.Length <= totalLength ? text : text.Substring(0, totalLength - 3) + "...";
            string data = needsTooltip ? "data-toggle=\"tooltip\" data-placement=\"top\" title=\"\" data-original-title=\"" + text + "\"" : "";
            return MvcHtmlString.Create(string.Format("<{0} {1}>{2}</{0}>", element, data, value));
        }

        public static IEnumerable<SelectListItem> GetWorks(this HtmlHelper html) {
            return HelperService.GetWorks().Select(w => new SelectListItem() { Value = w.Id.ToString(), Text = string.Format("{0} - {1}", w.Code, w.Name) });
        }

        public static IEnumerable<SelectListItem> GetWorkDescriptions(this HtmlHelper html) {
            return HelperService.GetWorkDescriptions().Select(w => new SelectListItem() { Value = w.Id.ToString("00"), Text = w.Name });
        }

        public static IEnumerable<SelectListItem> GetDepartments(this HtmlHelper html) {
            return HelperService.GetDepartments().Select(d => new SelectListItem() { Value = d.Id.ToString("00"), Text = d.Name });
        }

        public static IEnumerable<SelectListItem> GetSuppliers(this HtmlHelper html) {
            return HelperService.GetSuppliers().Select(d => new SelectListItem() { Value = d.CNPJ, Text = string.Format("{0} - {1}", d.CNPJ, d.Name) });
        }

        public static IEnumerable<SelectListItem> RequestTypes(this HtmlHelper html, string selected = "") {
            yield return new SelectListItem() { Value = "", Text = "Selecione" };
            yield return new SelectListItem() { Value = AnalysisRequest.MonthlyAnalysis, Text = "Análise Mensal", Selected = (AnalysisRequest.MonthlyAnalysis == selected) };
            yield return new SelectListItem() { Value = AnalysisRequest.Compliance, Text = "Compliance", Selected = (AnalysisRequest.Compliance == selected) };
            yield return new SelectListItem() { Value = AnalysisRequest.Litigation, Text = "Contencioso", Selected = (AnalysisRequest.Litigation == selected) };
            yield return new SelectListItem() { Value = AnalysisRequest.Homologation, Text = "Homologação", Selected = (AnalysisRequest.Homologation == selected) };
        }

        public static IEnumerable<SelectListItem> GetJudicialPhases(this HtmlHelper html) {
            yield return new SelectListItem() { Value = "", Text = "Selecione" };
            yield return new SelectListItem() { Value = "Knowledge", Text = "Conhecimento" };
            yield return new SelectListItem() { Value = "Recursion", Text = "Recursão" };
            yield return new SelectListItem() { Value = "Execution", Text = "Execução" };
        }

        public static IEnumerable<SelectListItem> GetLitigationTypes(this HtmlHelper html) {
            yield return new SelectListItem() { Value = "", Text = "Selecione" };
            yield return new SelectListItem() { Value = "Judicial", Text = "Judicial" };
            yield return new SelectListItem() { Value = "PreJudicial", Text = "Pré Judicial" };
            yield return new SelectListItem() { Value = "ExtraJudicial", Text = "Extra Judicial" };
        }

        public static IEnumerable<SelectListItem> GetSuccessProbabilityTypes(this HtmlHelper html) {
            yield return new SelectListItem() { Value = "", Text = "Selecione" };
            yield return new SelectListItem() { Value = "Possible", Text = "Possível" };
            yield return new SelectListItem() { Value = "Probable", Text = "Provável" };
            yield return new SelectListItem() { Value = "Remote", Text = "Remoto" };
        }
    }
}