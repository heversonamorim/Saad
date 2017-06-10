using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Saad.Models {
    public class FinalReportViewModel {

        public static int DisapprovalRate = int.Parse(ConfigurationManager.AppSettings["DisapprovalRate"]);
        public static int Level2ApprovalRate = int.Parse(ConfigurationManager.AppSettings["Level2ApprovalRate"]);
        public static int Level1ApprovalRate = int.Parse(ConfigurationManager.AppSettings["Level1ApprovalRate"]);

        public class Parecer {

            public string LabelName { get; set; }
            public int BaseEvaluationSum { get; set; }
            public int EvaluationSum { get; set; }

            public decimal EvaluationPercentage {
                get {
                    return (decimal)EvaluationSum / (decimal)BaseEvaluationSum;
                }
            }

            public int RiskLevel {
                get {
                    if (EvaluationPercentage * 100 < DisapprovalRate) {
                        return 4;
                    } else if (EvaluationPercentage * 100 < Level2ApprovalRate) {
                        return 3;
                    } else if (EvaluationPercentage * 100 < Level1ApprovalRate) {
                        return 2;
                    } else {
                        return 1;
                    }

                }
            }

            public string RiskLevelCss {
                get {
                    if (RiskLevel == 4)
                        return "bg-danger";
                    if (RiskLevel == 2 || RiskLevel == 3)
                        return "bg-warning";
                    return "bg-success";
                }
            }

        }

        public FinalReportViewModel(AnalysisRequest request, IList<AnalysisRequest> history) {
            GenerateData(request);
            History = history;
        }

        public int? HomologonEvalution {
            get {
                var homolon = Request.Documents.FirstOrDefault(r => r.DocumentReference.EndsWith("HOMOLON"));
                if (homolon != null)
                    return homolon.EvaluationPoints;
                return null;
            }
        }

        public IList<AnalysisRequest> History { get; set; }

        public AnalysisRequest Request { get; set; }

        public IEnumerable<Parecer> ParecerList { get; set; }

        public int BaseEvaluationSum { get; set; }

        public int EvaluationSum { get; set; }

        public int RiskLevel {
            get {
                if (EvaluationPercentage * 100 < DisapprovalRate) {
                    return 4;
                } else if (EvaluationPercentage * 100 < Level2ApprovalRate) {
                    return 3;
                } else if (EvaluationPercentage * 100 < Level1ApprovalRate) {
                    return 2;
                } else {
                    return 1;
                }

            }
        }

        public string RiskLevelName {
            get {
                if (RiskLevel == 2)
                    return "Moderado";
                if (RiskLevel == 3)
                    return "Moderado sob alerta";
                if (RiskLevel == 4)
                    return "Crítico";

                return "Normal";
            }
        }

        public string RiskLevelCss {
            get {
                if (RiskLevel == 4)
                    return "bg-danger";
                if (RiskLevel == 2 || RiskLevel == 3)
                    return "bg-warning";
                return "bg-success";
            }
        }
        public decimal EvaluationPercentage {
            get {
                return (decimal)EvaluationSum / (decimal)BaseEvaluationSum;
            }
        }

        private void GenerateData(AnalysisRequest request) {
            Request = request;

            var data = from doc in request.Documents
                       where !doc.DocumentReference.Contains("HOMOLON")
                       group doc by doc.Type into g
                       select new Parecer() {
                           LabelName = g.Key,
                           BaseEvaluationSum = g.Sum(d => d.MaximumEvaluationPoints.GetValueOrDefault(10)),
                           EvaluationSum = g.Sum(d => d.EvaluationPoints.GetValueOrDefault(0))
                       };

            var validDocuments = request.Documents.Where(r => !r.DocumentReference.Contains("HOMOLON"));

            BaseEvaluationSum = validDocuments.Sum(d => d.MaximumEvaluationPoints.GetValueOrDefault(10));
            EvaluationSum = validDocuments.Sum(d => d.EvaluationPoints.GetValueOrDefault(0));

            ParecerList = data;

        }

    }
}