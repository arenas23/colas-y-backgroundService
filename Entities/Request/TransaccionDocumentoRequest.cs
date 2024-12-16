using Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request
{
    public class TransaccionDocumentoRequest : TransaccionRequest, IValidatableObject
    {
        [Required(ErrorMessage = ValidatorMessage.Required)]
        public DateTime FechaGeneracion { get; set; }
        public MetodoPagoRequest? MetodoPago { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validCodes = new List<string> { "40", "07" };
            if (!validCodes.Contains(TipoDocumento))
            {
                yield return new ValidationResult(ValidatorMessage.InvalidTypeDocument, new[] { nameof(TipoDocumento) });
            }

            if (Total <= 0M && (ValorReferencia == null || ValorReferencia <= 0))
            {
                yield return new ValidationResult(ValidatorMessage.PriceReference, new[] { nameof(ValorReferencia) });

            }
        }
    }
}
