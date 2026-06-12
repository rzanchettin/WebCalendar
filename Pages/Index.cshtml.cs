using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;

namespace WebCalendar.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _env;

        public IndexModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        [BindProperty]
        public int? Year { get; set; }

        public List<MonthView> Months { get; set; }
        public List<Holiday> Holidays { get; set; }
        public List<Birthday> Birthdays { get; set; }
        public List<Note> Notes { get; set; }

        public void OnGet()
        {
            // Pre-fill the year input with the current year and build the calendar
            Year = DateTime.Now.Year;
            LoadHolidays();
            LoadBirthdays();
            LoadNotes();
            Months = BuildYear(Year.Value);
        }

        public void OnPost()
        {
            if (Year.HasValue && Year.Value >= 1 && Year.Value <= 9999)
            {
                LoadHolidays();
                LoadBirthdays();
                LoadNotes();
                Months = BuildYear(Year.Value);
            }
            else
            {
                ModelState.AddModelError("Year", "Enter a valid 4-digit year between 1 and 9999.");
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

            // Weeks: list of 7-day rows, Sunday..Saturday. Use null for empty cells.
            var weeks = new List<List<int?>>();
            var currentWeek = new List<int?>();

            // Add leading empty days until Sunday-based first day index
            int leading = (int)first.DayOfWeek; // Sunday = 0
            for (int i = 0; i < leading; i++) currentWeek.Add(null);

            for (int d = 1; d <= days; d++)
            {
                currentWeek.Add(d);
                if (currentWeek.Count == 7)
                {
                    weeks.Add(currentWeek);
                    currentWeek = new List<int?>();
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
                Holidays = Holidays?.Where(h => h.month == month).ToList(),
                Birthdays = Birthdays?.Where(b => b.month == month).ToList()
            };
        }

        private void LoadHolidays()
        {
            try
            {
                var path = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "holidays.json");
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    Holidays = System.Text.Json.JsonSerializer.Deserialize<List<Holiday>>(json);

                    // Compatibilidade retroativa: adicionar 'recurring' se não existir
                    if (Holidays != null)
                    {
                        foreach (var h in Holidays)
                        {
                            // Se recurring não foi setado, use o padrão
                            if (h.recurring == false && h.year == null)
                            {
                                // Assume true para dados antigos
                                h.recurring = true;
                            }
                        }
                    }
                }
                else
                {
                    Holidays = new List<Holiday>();
                }
            }
            catch
            {
                Holidays = new List<Holiday>();
            }
        }

        private void LoadBirthdays()
        {
            try
            {
                var path = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "birthday.json");
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    Birthdays = System.Text.Json.JsonSerializer.Deserialize<List<Birthday>>(json);

                    // Compatibilidade retroativa: adicionar 'recurring' se não existir
                    if (Birthdays != null)
                    {
                        foreach (var b in Birthdays)
                        {
                            // Aniversários sempre são recorrentes
                            b.recurring = true;
                        }
                    }
                }
                else
                {
                    Birthdays = new List<Birthday>();
                }
            }
            catch
            {
                Birthdays = new List<Birthday>();
            }
        }

        private void LoadNotes()
        {
            try
            {
                var path = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "notes.json");
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    Notes = System.Text.Json.JsonSerializer.Deserialize<List<Note>>(json);

                    // Compatibilidade retroativa
                    if (Notes != null)
                    {
                        foreach (var n in Notes)
                        {
                            // Se recurring não foi setado, use o padrão
                            if (n.recurring == false && n.year == null)
                            {
                                n.recurring = true;
                            }
                        }
                    }
                }
                else
                {
                    Notes = new List<Note>();
                }
            }
            catch
            {
                Notes = new List<Note>();
            }
        }

        public class MonthView
        {
            public int Month { get; set; }
            public int Year { get; set; }
            public string Name { get; set; }
            public List<List<int?>> Weeks { get; set; }
            public List<Holiday> Holidays { get; set; }
            public List<Birthday> Birthdays { get; set; }
        }

        public class Holiday
        {
            public int day { get; set; }
            public int month { get; set; }
            public string name { get; set; }
            public bool recurring { get; set; } = true; // true = toda ano, false = apenas um ano específico
            public int? year { get; set; } // preenchido apenas se recurring = false
        }

        public class Birthday
        {
            public int day { get; set; }
            public int month { get; set; }
            public string name { get; set; }
            public bool recurring { get; set; } = true; // sempre true para aniversários
        }

        public class Note
        {
            public int day { get; set; }
            public int month { get; set; }
            public string name { get; set; }
            public bool recurring { get; set; } = true; // true = toda ano, false = apenas um ano específico
            public int? year { get; set; } // preenchido apenas se recurring = false
        }
    }
}
