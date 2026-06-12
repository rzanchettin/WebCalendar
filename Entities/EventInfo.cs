namespace WebCalendar.Entities
{
    public class EventInfo
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool Recurring { get; set; }
    }
}
