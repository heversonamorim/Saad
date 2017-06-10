using Saad.Lib.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Workers {
    class Program {

        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args) {

            try {
               
                var analysisRequestSerivce = new AnalysisRequestService();
                analysisRequestSerivce.SendExpiringPeriodToSendDocumentsNotification(log);

                var analysisRequestMonthlyControl = new AnalysisRequestMonthlyControlService();
                analysisRequestMonthlyControl.GenerateMonthlyControl(log);

            } catch (Exception ex) {
                log.Error("Main Error", ex);
            }

        }
    }
}
