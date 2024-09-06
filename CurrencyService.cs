using currency_rate;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace CurrencyLoader
{
    public class CurrencyService
    {
        // Sourse url
        private const string UrlStr = "https://www.cbr.ru/scripts/XML_daily.asp";
        // CultureInfo parse decimal
        private readonly CultureInfo cultureInfo = new("ru-RU");
        // Encoding parse string
        private readonly Encoding encoding = Encoding.GetEncoding("windows-1252");
        private readonly DateOnly dateNow = DateOnly.FromDateTime(DateTime.Now); 


        // Сохраняем данные за сегоня в бд
        public async Task DailyAsync()
        {
            var rates = await GetDailyRatesAsync();
            SaveDb(rates);
        }

        // Сохраняем данные за месяц в бд
        public async Task FillRatesForLastMonthAsync()
        {
            var startDate = dateNow.AddMonths(-1);

            while (startDate <= dateNow)
            {
                var rates = await GetRatesForDateAsync(startDate);
                SaveDb(rates);
                startDate = startDate.AddDays(1);
            }
        }

        // Читаем курс на последнюю дату с учетом кодировки
        private async Task<List<CurrencyRate>> GetDailyRatesAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(UrlStr);
                response.EnsureSuccessStatusCode();
                var responseStream = await response.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(responseStream, encoding))
                {
                    var responseBody = await reader.ReadToEndAsync();
                    return ParseRates(responseBody);
                }
            }
        }

        // Читаем курс на заданую дату с учетом кодировки
        private async Task<List<CurrencyRate>> GetRatesForDateAsync(DateOnly date)
        {
            var url = $"{UrlStr}?date_req={date:dd/MM/yyyy}";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var responseStream = await response.Content.ReadAsStreamAsync();
                using (var reader = new System.IO.StreamReader(responseStream, encoding))
                {
                    var responseBody = await reader.ReadToEndAsync();
                    return ParseRates(responseBody);
                }
            }
        }

        //Парсер xml в CurrencyRate[]
        private List<CurrencyRate> ParseRates(string xml)
        {
            var document = XDocument.Parse(xml);
            var rates = new List<CurrencyRate>();


            //var date = DateTime.ParseExact(document.Root.Attribute("Date").Value, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var date = DateOnly.ParseExact(document.Root.Attribute("Date").Value, "dd.MM.yyyy");

            foreach (var element in document.Descendants("Valute"))
            {
                var rate = new CurrencyRate
                {
                    ID = element.Attribute("ID").Value,
                    NumCode = ushort.Parse(element.Element("NumCode").Value),
                    CharCode = element.Element("CharCode").Value,
                    Nominal = int.Parse(element.Element("Nominal").Value),
                    Name = element.Element("Name").Value,
                    Value = decimal.Parse(element.Element("Value").Value, cultureInfo),
                    VunitRate = double.Parse(element.Element("VunitRate").Value, cultureInfo),
                    Date = date
                };
                rates.Add(rate);
            }

            return rates;
        }

        // Сохраняем курсы валют в базу данных
        private async void SaveDb(List<CurrencyRate> rates)
        {
            using (var _context = new CurrencyRateContext())
            {
                foreach (var item in rates)
                {
                    var currentRate = _context.Rates.FirstOrDefault(e => e.Date == item.Date && e.CurrencyID == item.ID);

                    if (currentRate is not null)
                        continue;

                    var exist = _context.Currencies.Any(e => e.CurrencyID == item.ID);

                    if (!exist)
                        _context.Currencies.Add(new Currency()
                        {
                            CurrencyID = item.ID,
                            NumCode = item.NumCode,
                            CharCode = item.CharCode,
                            Nominal = item.Nominal,
                            Name = item.Name
                        });

                    _context.Rates.Add(new Rate()
                    {
                        CurrencyID = item.ID,
                        Value = item.Value,
                        VunitRate = item.VunitRate,
                        Date = item.Date
                    });
                    _context.SaveChangesAsync();
                }
            }
        }
    }
}
