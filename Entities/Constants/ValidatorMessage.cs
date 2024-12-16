using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Constants
{
    public class ValidatorMessage
    {
        public const string Required = "Campo obligatorio.";
        public const string RangeDecimal = "Valor invalido. Máximo 12 dígitos enteros y 2 dígitos decimales.";
        public const string StringLength = "El campo debe contener entre {2} y {1} caracteres.";
        public const string MaxLength = "El campo debe contener máximo {1} caracteres.";
        public const string MinLenght = "El campo debe contener mínimo {1} caracteres.";
        public const string MinItem = "Se debe reportar al menos un ítem.";
        public const string RangePercentage = "El campo debe estar entre 0 y 100, con un máximo 2 decimales.";
        public const string RangeDate = "La fecha inicial no puede ser posterior a la fecha final.";
        public const string Range = "El valor debe estar entre {1} y {2}";
        public const string InvalidCode = "Código DIAN invalido.";
        public const string InvalidInteger = "Valor invalido.";
        public const string RangeDiference = "Valor invalido. Máximo 1 dígitos enteros y 2 dígitos decimales.";
        public const string TributeListInvalid = "Se han reportado más de una vez el mismo tributo con el(s) código(s) ";
        public const string InvalidTypeDocument = "El tipo de documento invalido, el tipo de documento no esta habilitado.";
        public const string InvalidYear = "El año reportado no es valido.";
        public const string LimitDaysDates = "La diferencia entre fechas es mayor a 30 días.";
        public const string LimitSevenDaysDates = "La diferencia entre fechas es mayor a 7 días.";
        public const string PriceReference = "Es necesario reportar un valor de referencia mayor a cero cuando el ítem a facturar tiene un valor de cero.";
    }
}
