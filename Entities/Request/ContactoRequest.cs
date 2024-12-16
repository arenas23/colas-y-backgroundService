using Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request
{
    public class ContactoRequest
    {
        [StringLength(450, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string? Nombre { get; set; }

        [StringLength(100, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string? NumeroTelefonico { get; set; }

        [StringLength(100, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string? NumeroFax { get; set; }

        [StringLength(500, MinimumLength = 5, ErrorMessage = ValidatorMessage.StringLength)]
        public string? Nota { get; set; }
    }
}
