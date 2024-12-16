using Domain.Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Request
{
    public class NumeracionEquivalenteRequest : IValidatableObject
    {
        [StringLength(6, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string Prefijo { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        public int? Consecutivo { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Consecutivo == null || Consecutivo < 1)
                yield return new ValidationResult(ValidatorMessage.InvalidInteger, new[] { nameof(Consecutivo) });
        }
    }
}
