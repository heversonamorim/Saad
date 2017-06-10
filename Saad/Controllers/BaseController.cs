using Saad.Helpers;
using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Saad.Controllers {
    public class BaseController : Controller {

        protected ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager {
            get {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set {
                _userManager = value;
            }
        }

        private ApplicationUser loggedUser;
        public ApplicationUser LoggedUser {
            get {
                if (loggedUser == null && User.Identity.IsAuthenticated) {
                    loggedUser = UserManager.FindByEmail(User.Identity.Name);
                }
                return loggedUser;
            }
        }

        public void AddMessage(MessageType type, string message) {
            if (TempData.ContainsKey(type.Name)) {
                if (!string.IsNullOrWhiteSpace(TempData[type.Name] as string))
                    TempData[type.Name] += "<br/>";
                TempData[type.Name] += message;
            } else {
                TempData.Add(type.Name, message);
            }
        }

        public void AddErrors(DbEntityValidationException ex) {
            foreach (var error in ex.EntityValidationErrors) {
                foreach (var e in error.ValidationErrors) {
                    ModelState.AddModelError("", e.ErrorMessage);
                }
            }
        }

    }
}