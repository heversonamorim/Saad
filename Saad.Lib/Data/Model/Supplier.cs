using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saad.Lib.Data.Model {
    public class Supplier {

        public class BriefSupplier {
            public string CNPJ { get; set; }
            public string Name { get; set; }
        }

        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CNPJ { get; set; }

        [Required]
        [StringLength(255, ErrorMessage="Nome deve possuir até 255 caraceteres")]
        public string Name { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Nome Fantasia deve possuir até 255 caraceteres")]
        public string FantasyName { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Nome do contato deve possuir até 255 caraceteres")]
        public string MainContactName { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Telefone deve possuir até 20 caraceteres")]
        public string MainContactPhone { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Celular deve possuir até 20 caraceteres")]
        public string MobilePhone { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "E-mail deve possuir até 100 caraceteres")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string MainContactEmail { get; set; }

        [Required]
        [StringLength(9, ErrorMessage = "CEP deve possuir até 9 caraceteres")]
        public string CEP { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Endereço deve possuir até 255 caraceteres")]
        public string Street { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Número deve possuir até 20 caraceteres")]
        public string Number { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Bairro deve possuir até 50 caraceteres")]
        public string District { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Cidade deve possuir até 100 caraceteres")]
        public string City { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Estado deve possuir até 2 caraceteres")]
        public string UF { get; set; }

    }
}
