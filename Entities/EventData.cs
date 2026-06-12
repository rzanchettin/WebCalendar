namespace WebCalendar.Entities
{
    public class EventData
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool Recurring { get; set; }
    }

}
