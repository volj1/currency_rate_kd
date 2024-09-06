using currency_rate;
using System.Text;

namespace CurrencyLoader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var _context = new CurrencyRateContext())
            {
                _context.Database.EnsureCreated();
            }
            var service = new CurrencyService();

            await service.DailyAsync();

            await service.FillRatesForLastMonthAsync();

        }
    }
}