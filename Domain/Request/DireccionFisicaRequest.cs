using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Request
{
    public class DireccionFisicaRequest
    {
        [Required(ErrorMessage = ValidatorMessage.Required)]
        [MaxLength(300, ErrorMessage = ValidatorMessage.MaxLength)]
        public string Direccion { get; set; } = string.Empty;

        [MaxLength(6, ErrorMessage = ValidatorMessage.MaxLength)]
        public string? CodigoPostal { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        [MaxLength(5, ErrorMessage = ValidatorMessage.MaxLength)]
        public string Municipio { get; set; } = string.Empty;

    }
}
