using Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request
{
    public class MetodoPagoRequest
    {
        [Required(ErrorMessage = ValidatorMessage.Required)]
        [MaxLength(1, ErrorMessage = ValidatorMessage.MaxLength)]
        public string FormaPago { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(3, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string MedioPago { get; set; } = string.Empty;
        public DateTime? FechaVencimiento { get; set; }


    }
}
