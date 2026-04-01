using ModelCore.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelCore.Schema.TurnKey.Allowance
{
    public static class AllowanceExtensions
    {
    }

    public partial class DetailsProductItem
    {
        public String? DataNumber;
    }

    public partial class Allowance
    {
        public String? TxnCode;
    }
}
