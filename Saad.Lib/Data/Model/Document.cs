using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class Document {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Relevance { get; set; }

        public string Reference { get; set; }

        public string Details { get; set; }

        public int MaximumScore { get; set; }

        public DocumentKit DocumentKit { get; set; }

    }
}
