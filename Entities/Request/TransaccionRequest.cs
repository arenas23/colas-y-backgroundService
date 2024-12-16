using Domain.Entities.Constants;
using Domain.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request
{
    public class TransaccionRequest
    {
        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(2, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string TipoDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        public NumeracionEquivalenteRequest Numeracion { get; set; } = new();

        [StringLength(15, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string? Codigo { get; set; }

        [StringLength(300, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string? Descripcion { get; set; }

        [RegularExpression(RegularExpressions.RangeDecimal, ErrorMessage = ValidatorMessage.RangeDecimal)]
        public decimal? ValorReferencia { get; set; }

        [Required(ErrorMessage = ValidatorMessage.Required)]
        [RegularExpression(RegularExpressions.RangeDecimal, ErrorMessage = ValidatorMessage.RangeDecimal)]
        public decimal? Total { get; set; }

    }
}
