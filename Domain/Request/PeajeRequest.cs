using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Request
{
 public class PeajeRequest
    {
        [Required(ErrorMessage = ValidatorMessage.Required)]
        [StringLength(150, MinimumLength = 10, ErrorMessage = ValidatorMessage.StringLength)]
        public string CodigoUnico { get; set; } = string.Empty;

        [Required(ErrorMessage = ValidatorMessage.Required)]
        public ConcesionRequest DatosEmisor { get; set; } = new();

        public ActorRequest? DatosAdquirente { get; set; }

        [Required(ErrorMessage = ValidatorMessage.Required)]
        public TransaccionDocumentoRequest DatosDocumento { get; set; } = new();

        public List<AdicionalRequest> DatosAdicionales { get; set; } = new();
    }
