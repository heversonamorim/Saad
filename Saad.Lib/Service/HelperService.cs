using Saad.Lib.Data.EntityFramework;
using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Service {
    public class HelperService {

        private Context context;

        public HelperService() {
            context = new Context();
        }

        public IList<Work> GetWorks() {
            return context.Works.OrderBy(w => w.Name).ToList();
        }

        public IList<WorkDescription> GetWorkDescriptions() {
            return context.WorkDescriptions.OrderBy(w => w.Name).ToList();
        }

        public IList<Department> GetDepartments() {
            return context.Departments.OrderBy(d  => d.Name).ToList();
        }

        public IList<Supplier.BriefSupplier> GetSuppliers() {
            return context.Database.SqlQuery<Supplier.BriefSupplier>("select distinct CNPJ, Name from Suppliers order by Name").ToList();
        }

    }
}
