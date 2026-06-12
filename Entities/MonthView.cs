using static WebCalendar.Pages.IndexModel;

namespace WebCalendar.Entities
{
    public class MonthView
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<List<int?>> Weeks { get; set; } = [];
        public List<Event> Holidays { get; set; } = [];
        public List<Event> Birthdays { get; set; } = [];
        public List<Event> Notes { get; set; } = [];
    }
}
