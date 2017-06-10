using Microsoft.AspNet.Identity.EntityFramework;
using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Saad.Lib.Data.EntityFramework {
    //public class Context : IdentityDbContext<ApplicationUser> {
        public class Context : IdentityDbContext<ApplicationUser> {

        public DbSet<AnalysisRequest> AnalysisRequest { get; set; }

        public DbSet<AnalysisRequestLitigation> Litigations { get; set; }

        public DbSet<AnalysisRequestDocument> AnalysisRequestDocuments { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<DocumentKit> DocumentKits { get; set; }
        public DbSet<Document> Documents { get; set; }

        public DbSet<AnalysisRequestMonthlyControl> AnalysisRequestMonthlyControls { get; set; }

        public DbSet<DocumentStandardReason> DocumentStandardReasons { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Work> Works { get; set; }
        public DbSet<WorkDescription> WorkDescriptions { get; set; }

        public DbSet<Department> Departments { get; set; }

        public Context()
            : base("DefaultConnection", throwIfV1Schema: false) {
        }   

        public static Context Create() {
            return new Context();
        }

    }
}
