using Saad.Lib.Service;
using Saad.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Saad.Controllers {

    [Authorize]

    
    public class HomeController : Controller {

        public AnalysisRequestService analysisRequestService = new AnalysisRequestService();

        public ActionResult Index() {
            return View(new HomeDashboardViewModel(analysisRequestService.List(null, null, null, null), null, null, null));
        }

        [HttpPost]
        public ActionResult Index(HomeDashboardViewModel model) {
            return View(new HomeDashboardViewModel(analysisRequestService.List(model.StartDate, model.FinishDate, model.Supplier, model.WorkId), model.StartDate, model.FinishDate, model.Supplier));
        }

        public ActionResult About() {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact() {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult GetZipcodeData(string zipcode) {
            if (!string.IsNullOrWhiteSpace(zipcode)) {
                zipcode = zipcode.Replace("-", "");

                var client = new Correios.AtendeClienteClient();
                var result = client.consultaCEP(zipcode);

                return Json(result, JsonRequestBehavior.AllowGet);

            }

            return HttpNotFound();

        }

    }
}