using Saad.Lib.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Saad.Helpers {
    public static class ApplicationHelper {

        public static string[] NotifyHomologationRequestToEmails {
            get {
                return ConfigurationManager.AppSettings["SAAD.Notify.HomologationRequest"].Split(',', ';');
            }
        }

    }
}