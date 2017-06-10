using Saad.Lib.Data.EntityFramework;
using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Service {
    public class UserService {
        
        private Context context;

        public UserService () {
            context = new Context();
        }

        public ApplicationUser Get(string id) {
            return context.Users.FirstOrDefault(u => u.Id == id);
        }

        public IEnumerable<ApplicationUser> ListCustomerUsers() {
            return context.Users.Where(u => u.Customer != null);
        }

    }
}
