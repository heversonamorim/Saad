using Saad.Lib.Data.EntityFramework;
using Saad.Lib.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Service {
    public class DocumentKitService {

        private Context context;

        public DocumentKitService() {
            context = new Context();
        }

        public DocumentKit GetByCode(string code) {
            return context.DocumentKits.FirstOrDefault(k => k.Code == code);
        }

        public Document GetDocument(int id) {
            return context.Documents.FirstOrDefault(d => d.Id == id);
        }

        public IList<DocumentStandardReason> DocumentStandardList() {
            return context.DocumentStandardReasons.ToList();
        }
    }
}
