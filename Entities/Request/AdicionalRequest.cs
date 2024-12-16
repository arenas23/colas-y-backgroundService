using Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request
{
    public class AdicionalRequest
    {

        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(50, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(50, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string Valor { get; set; } = string.Empty;
    }
}
