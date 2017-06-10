using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Saad.Models {
    public class HomeDashboardViewModel {
        public IList<KeyValuePair<string, int>> HomologationData { get; set; }
        public IList<KeyValuePair<string, int>> MonthlyAnalysisData { get; set; }

        public IList<KeyValuePair<string, int>> LitigationData { get; set; }

        public IList<KeyValuePair<DateTime, decimal>> SupplierData { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? FinishDate { get; set; }

        public string Supplier { get; set; }
        public int WorkId { get; set; }

        public HomeDashboardViewModel() { }

        public HomeDashboardViewModel(IList<AnalysisRequest> requests, DateTime? startDate, DateTime? finishDate, string supplier) {
            HomologationData = (from request in requests
                               where request.Type == AnalysisRequest.Homologation
                               group request by request.Status.Name into g
                               select new KeyValuePair<string, int>(g.Key, g.Count())).ToList();

            MonthlyAnalysisData = (from request in requests
                                where request.Type == AnalysisRequest.MonthlyAnalysis
                                group request by request.Status.Name into g
                                select new KeyValuePair<string, int>(g.Key, g.Count())).ToList();

            LitigationData = (from request in requests
                              where request.Type == AnalysisRequest.Litigation && request.LitigationData != null
                              group request by request.LitigationData.LitigationTypeName into g
                              select new KeyValuePair<string, int>(g.Key, g.Count())).ToList();

            if (!string.IsNullOrWhiteSpace(supplier)) {
                SupplierData = (from request in requests
                                where request.ReferenceDate.HasValue
                                group request by request.ReferenceDate into g
                                select new KeyValuePair<DateTime, decimal>(g.Key.Value, g.Average(r => r.EvaluationPercentage.GetValueOrDefault()))).ToList();

            }

            StartDate = startDate;
            FinishDate = finishDate;
        }

        public string GetHomologationKeyArray() {
            return string.Join(",", HomologationData.Select(t => string.Format("\"{0}\"", t.Key)));
        }

        public string GetHomologationValueArray() {
            return string.Join(",", HomologationData.Select(t => t.Value));
        }

        public string GetMonthlyAnalysisKeyArray() {
            return string.Join(",", MonthlyAnalysisData.Select(t => string.Format("\"{0}\"", t.Key)));
        }

        public string GetMonthlyAnalysisValueArray() {
            return string.Join(",", MonthlyAnalysisData.Select(t => t.Value));
        }

        public string GetLitigationKeyArray() {
            return string.Join(",", LitigationData.Select(t => string.Format("\"{0}\"", t.Key)));
        }

        public string GetLitigationValueArray() {
            return string.Join(",", LitigationData.Select(t => t.Value));
        }

        public string GetSupplierKeyArray() {
            if (SupplierData != null)
                return string.Join(",", SupplierData.Select(t => string.Format("\"{0}\"", t.Key.ToString("MM/yyyy"))));
            return string.Empty;
        }

        public string GetSupplierValueArray() {
            if (SupplierData != null)
                return string.Join(",", SupplierData.Select(t => t.Value.ToString("0.0", new CultureInfo("en-US"))));
            return null;
        }

    }
}