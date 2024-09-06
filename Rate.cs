using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace currency_rate
{
    public class Rate
    {
        public string CurrencyID { get; set; } = null!;
        public decimal Value { get; set; }
        public double VunitRate { get; set; }
        public DateOnly Date { get; set; }

        public Currency Currency { get; set; }
    }
}
