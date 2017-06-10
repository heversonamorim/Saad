using Saad.Lib.Data.EntityFramework;
using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Service {
    public class CustomerService {

        private Context context;

        public CustomerService () {
            context = new Context();
        }

        public Customer Get(int id) {
            return context.Customers.FirstOrDefault(c => c.Id == id);
        }

        public IEnumerable<Customer> List() {
            return context.Customers.OrderBy(c => c.Name);
        }


    }
}
