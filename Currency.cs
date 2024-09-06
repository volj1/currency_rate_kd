using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace currency_rate
{
    public class Currency
    {
        public string CurrencyID { get; set; } = null!;
        public int NumCode { get; set; }
        public string CharCode { get; set; }
        public int Nominal { get; set; }
        public string Name { get; set; }

        public List<Rate> Rates { get; set; }
    }
}
