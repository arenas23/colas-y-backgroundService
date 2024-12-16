using Entities.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Request
{
    public class ActorRequest
    {
        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(2, MinimumLength = 2, ErrorMessage = ValidatorMessage.StringLength)]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(12, MinimumLength = 5, ErrorMessage = ValidatorMessage.StringLength)]
        public string NumeroIdentificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(450, MinimumLength = 5, ErrorMessage = ValidatorMessage.StringLength)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(45, MinimumLength = 1, ErrorMessage = ValidatorMessage.StringLength)]
        public string CorreoElectronico { get; set; } = string.Empty;

        public DireccionFisicaRequest? DireccionFisica { get; set; }

        [MaxLength(1, ErrorMessage = ValidatorMessage.MaxLength)]
        public string? TipoOrganizacionJuridica { get; set; }
        public List<string> ActividadEconomica { get; set; } = new();
        public List<string> ResponsabilidadFiscal { get; set; } = new();

        [StringLength(2, MinimumLength = 2, ErrorMessage = ValidatorMessage.StringLength)]
        public string? ResponsabilidadTributaria { get; set; }
        public ContactoRequest DatosContacto { get; set; } = new();
    }
}
