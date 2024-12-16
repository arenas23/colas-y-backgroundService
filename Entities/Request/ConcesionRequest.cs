using Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request
{
    public class ConcesionRequest
    {
        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(2, MinimumLength = 2, ErrorMessage = ValidatorMessage.StringLength)]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(12, MinimumLength = 5, ErrorMessage = ValidatorMessage.StringLength)]
        public string NumeroIdentificacion { get; set; } = string.Empty;

    }
}

