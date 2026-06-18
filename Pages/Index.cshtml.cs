using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using WebCalendar.Entities;

namespace WebCalendar.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Construtor da página, recebe o ambiente de hospedagem para acessar os arquivos
        /// </summary>
        /// <param name="env"></param>
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
        public int DisplayYear { get; set; }
        public List<MonthView> Months { get; set; }
        public List<Event> Holidays { get; set; }
        public List<Event> Birthdays { get; set; }
        public List<Event> Notes { get; set; }

        public List<EventItem> BirthdaysVm { get; set; } = [];
        public List<EventItem> HolidaysVm { get; set; } = [];
        public List<EventItem> NotesVm { get; set; } = [];

        public List<TempEvent> AllBirthdays { get; set; } = [];
        public List<TempEvent> AllHolidays { get; set; } = [];
        public List<TempEvent> AllNotes { get; set; } = [];

        /// <summary>
        /// Inicializa a página, carregando os eventos para o ano atual e 
        /// construindo a estrutura do calendário para exibição
        /// </summary>
        public void OnGet()
        {
            Year = DateTime.Now.Year;
            CarregarTodosEventos();
            Months = ConstruirMesesCalendario(Year!.Value);
            ProcessarEventos();
        }

        /// <summary>
        /// Valida o ano e recarrega os dados para o ano especificado
        /// </summary>
        public void OnPost()
        {
            if (Year.HasValue && Year.Value >= 1 && Year.Value <= 9999)
            {
                CarregarTodosEventos();
                Months = ConstruirMesesCalendario(Year!.Value);
                ProcessarEventos();
            }
            else
            {
                ModelState.AddModelError("Year", "Ano deve conter até 4 dígitos numéricos");
            }
        }

        /// <summary>
        /// Carrega os eventos e constrói a estrutura do ano para exibição
        /// </summary>
        private void CarregarTodosEventos()
        {
            Holidays = CarregarEvento("holidays");
            Birthdays = CarregarEvento("birthdays");
            Notes = CarregarEvento("notes");
        }

        /// <summary>
        /// Processa os eventos carregados, calculando o dia da 
        /// semana e organizando-os para exibição
        /// </summary>
        private void ProcessarEventos()
        {
            DisplayYear = (Year.GetValueOrDefault() > 0) ? Year.GetValueOrDefault() : DateTime.Now.Year;

            if (Months == null) return;

            foreach (var mm in Months)
            {
                if (mm.Birthdays != null)
                {
                    foreach (var b in mm.Birthdays)
                    {
                        AllBirthdays.Add(new TempEvent
                        {
                            Month = mm.Month,
                            Day = b.Day,
                            Name = b.Name,
                            Dow = DowAbbrev(DisplayYear, mm.Month, b.Day),
                            MonthAbbrev = MonthAbbrev(mm.Month),
                            Recurring = true
                        });
                    }
                }

                if (mm.Holidays != null)
                {
                    foreach (var h in mm.Holidays)
                    {
                        AllHolidays.Add(new TempEvent
                        {
                            Year = mm.Year,
                            Month = mm.Month,
                            Day = h.Day,
                            Name = h.Name,
                            Dow = DowAbbrev(DisplayYear, mm.Month, h.Day),
                            MonthAbbrev = MonthAbbrev(mm.Month),
                            Recurring = h.Recurring
                        });
                    }
                }

                if (mm.Notes != null)
                {
                    foreach (var n in mm.Notes)
                    {
                        AllNotes.Add(new TempEvent
                        {
                            Year = mm.Year,
                            Month = mm.Month,
                            Day = n.Day,
                            Name = n.Name,
                            Dow = DowAbbrev(DisplayYear, mm.Month, n.Day),
                            MonthAbbrev = MonthAbbrev(mm.Month),
                            Recurring = n.Recurring
                        });
                    }
                }
            }

            AllBirthdays = AllBirthdays.OrderBy(x => x.Month).ThenBy(x => x.Day).ToList();
            AllHolidays = AllHolidays.OrderBy(x => x.Month).ThenBy(x => x.Day).ToList();
            AllNotes = AllNotes.OrderBy(x => x.Month).ThenBy(x => x.Day).ToList();

            BirthdaysVm = AllBirthdays.Select(ToEventItem).ToList();

            HolidaysVm = AllHolidays
                .Select(ToEventItem)
                .Where(x => x.Recurring || x.Year == DateTime.Now.Year)
                .ToList();

            NotesVm = AllNotes
                .Select(ToEventItem)
                .Where(x => x.Recurring || x.Year == DateTime.Now.Year)
                .ToList();
        }

        /// <summary>
        /// Converte um TempEvent para EventItem, preparando-o para exibição na interface
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private EventItem ToEventItem(TempEvent x)
        {
            return new EventItem
            {
                Day = x.Day,
                Month = x.Month,
                Name = x.Name,
                Dow = x.Dow,
                MonthAbbrev = x.MonthAbbrev,
                Year = x.Year.GetValueOrDefault(),
                Recurring = x.Recurring
            };
        }

        /// <summary>
        /// Obtém a abreviação do dia da semana para um determinado ano, mês e dia
        /// </summary>
        /// <param name="ano"></param>
        /// <param name="mes"></param>
        /// <param name="dia"></param>
        /// <returns></returns>
        private static string DowAbbrev(int ano, int mes, int dia)
        {
            try
            {
                var dow = new DateTime(ano, mes, dia).DayOfWeek;
                return dow switch
                {
                    DayOfWeek.Monday => "seg",
                    DayOfWeek.Tuesday => "ter",
                    DayOfWeek.Wednesday => "qua",
                    DayOfWeek.Thursday => "qui",
                    DayOfWeek.Friday => "sex",
                    DayOfWeek.Saturday => "sáb",
                    DayOfWeek.Sunday => "dom",
                    _ => ""
                };
            }
            catch { return ""; }
        }

        /// <summary>
        /// Obtém a abreviação do mês para um determinado número de mês
        /// </summary>
        /// <param name="mes"></param>
        /// <returns></returns>
        private static string MonthAbbrev(int mes) => mes switch
        {
            1 => "Jan",
            2 => "Fev",
            3 => "Mar",
            4 => "Abr",
            5 => "Mai",
            6 => "Jun",
            7 => "Jul",
            8 => "Ago",
            9 => "Set",
            10 => "Out",
            11 => "Nov",
            12 => "Dez",
            _ => mes.ToString()
        };

        /// <summary>
        /// Constrói a estrutura de meses para um determinado ano, 
        /// incluindo os dias e eventos associados
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        private List<MonthView> ConstruirMesesCalendario(int year)
        {
            var months = new List<MonthView>();
            for (int m = 1; m <= 12; m++)
            {
                months.Add(BuildMonth(year, m));
            }
            return months;
        }

        /// <summary>
        /// Constrói a estrutura de um mês específico, organizando os dias em
        /// semanas e associando os eventos correspondentes
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        private MonthView BuildMonth(int year, int month)
        {
            var first = new DateTime(year, month, 1);
            int days = DateTime.DaysInMonth(year, month);

            var weeks = new List<List<int?>>();
            var currentWeek = new List<int?>();

            int leading = (int)first.DayOfWeek;
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
                while (currentWeek.Count < 7) currentWeek.Add(null);
                weeks.Add(currentWeek);
            }

            return new MonthView
            {
                Month = month,
                Year = year,
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)),
                Weeks = weeks,
                Holidays = Holidays?.Where(h => h.Month == month).ToList() ?? [],
                Birthdays = Birthdays?.Where(b => b.Month == month).ToList() ?? [],
                Notes = Notes?.Where(n => n.Month == month).ToList() ?? []
            };
        }

        /// <summary>
        /// Carrega os eventos de um tipo específico (feriados, aniversários ou notas) 
        /// a partir de um arquivo JSON localizado na pasta wwwroot
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        private List<Event> CarregarEvento(string eventType)
        {
            List<Event> eventList = [];
            try
            {
                var path = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), $"{eventType}.json");

                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    eventList = System.Text.Json.JsonSerializer.Deserialize<List<Event>>(json) ?? [];

                    foreach (var h in eventList)
                    {
                        if (h.Recurring == false && h.Year == null)
                        {
                            h.Recurring = true;
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

    /// <summary>
    /// Classe auxiliar para representar eventos temporários durante o 
    /// processamento dos dados, facilitando a organização e exibição 
    /// dos eventos na interface
    /// </summary>
    public class TempEvent
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Dow { get; set; } = string.Empty;
        public string MonthAbbrev { get; set; } = string.Empty;
        public int? Year { get; set; }
        public bool Recurring { get; set; }
    }
}