using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Helpers
{
    public class RegularExpressions
    {
        public const string RangeDecimal = @"^\d{1,12}([.,]\d{1,2})?$";
        public const string RangePercentage = @"^(100(\.0{1,2})?|0*?[0-9]{0,2}([.,]\d{1,2})?)$";
        public const string RangeDiference = @"^-?\d{1,1}([.,]\d{1,2})?$";
    }
}
