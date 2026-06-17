namespace WebCalendar.Entities
{
    public class EventItem
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Dow { get; set; } = string.Empty; // day of week
        public string MonthAbbrev { get; set; } = string.Empty; // month abbreviation
        public bool Recurring { get; set; }
        public int Year { get; set; }
    }
}
