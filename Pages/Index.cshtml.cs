using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using WebCalendar.Entities;

namespace WebCalendar.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _env;

        public IndexModel(IWebHostEnvironment env)
        {
            _env = env;
            Months = [];
            Holidays = [];
            Birthdays = [];
            Notes = [];
        }

        [BindProperty]
        public int? Year { get; set; }
        public List<MonthView> Months { get; set; }
        public List<Event> Holidays { get; set; }
        public List<Event> Birthdays { get; set; }
        public List<Event> Notes { get; set; }

        public void OnGet()
        {
            Year = DateTime.Now.Year;
            Holidays = LoadEvents("holidays");
            Birthdays = LoadEvents("birthdays");
            Notes = LoadEvents("notes");
            Months = BuildYear(Year.Value);
        }

        public void OnPost()
        {
            if (Year.HasValue && Year.Value >= 1 && Year.Value <= 9999)
            {
                Holidays = LoadEvents("holidays");
                Birthdays = LoadEvents("birthdays");
                Notes = LoadEvents("notes");
                Months = BuildYear(Year.Value);
            }
            else
            {
                ModelState.AddModelError("Year", "Enter a valid 4-digit Year between 1 and 9999.");
            }
        }

        private List<MonthView> BuildYear(int year)
        {
            var months = new List<MonthView>();
            for (int m = 1; m <= 12; m++)
            {
                months.Add(BuildMonth(year, m));
            }
            return months;
        }

        private MonthView BuildMonth(int year, int month)
        {
            var first = new DateTime(year, month, 1);
            int days = DateTime.DaysInMonth(year, month);

            // Weeks: list of 7-Day rows, Sunday..Saturday. Use null for empty cells.
            var weeks = new List<List<int?>>();
            var currentWeek = new List<int?>();

            // Add leading empty days until Sunday-based first Day index
            int leading = (int)first.DayOfWeek; // Sunday = 0
            for (int i = 0; i < leading; i++) currentWeek.Add(null);

            for (int d = 1; d <= days; d++)
            {
                currentWeek.Add(d);
                if (currentWeek.Count == 7)
                {
                    weeks.Add(currentWeek);
                    currentWeek = [];
                }
            }

            if (currentWeek.Count > 0)
            {
                // fill remaining to 7
                while (currentWeek.Count < 7) currentWeek.Add(null);
                weeks.Add(currentWeek);
            }

            return new MonthView
            {
                Month = month,
                Year = year,
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)),
                Weeks = weeks,
                Holidays = Holidays?.Where(h => h.Month == month).ToList() ?? [],
                Birthdays = Birthdays?.Where(b => b.Month == month).ToList() ?? [],
                Notes = Notes?.Where(n => n.Month == month).ToList() ?? []
            };
        }

        private List<Event> LoadEvents(string eventType)
        {
            List<Event> eventList = [];
            try
            {
                var path = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), $"{eventType}.json");
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    eventList = System.Text.Json.JsonSerializer.Deserialize<List<Event>>(json) ?? [];

                    // Compatibilidade retroativa: adicionar 'Recurring' se não existir
                    if (eventList != null)
                    {
                        foreach (var h in eventList)
                        {
                            // Se Recurring não foi setado, use o padrão
                            if (h.Recurring == false && h.Year == null)
                            {
                                // Assume true para dados antigos
                                h.Recurring = true;
                            }
                        }
                    }
                }
            }
            catch
            {
                eventList = [];
            }

            return eventList ?? [];

        }
    }
}
