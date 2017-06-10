using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class DocumentKit {

        public DocumentKit() {
            Documents = new Collection<Document>();
        }

        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Code { get; set; }

        public virtual ICollection<Document> Documents { get; set; }

    }
}
