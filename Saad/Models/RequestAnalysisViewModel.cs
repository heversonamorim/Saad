using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Saad.Models {
    public class RequestAnalysisViewModel {

        public AnalysisRequest Request { get; set; }

        public HttpPostedFileBase Contract { get; set; }

        [Required]
        public IList<int> WorkDescriptions { get; set; }

        public bool CreateAnother { get; set; }

        [Required]
        public string ReferenceDate { get; set; }
    }
}