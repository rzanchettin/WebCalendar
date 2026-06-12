namespace WebCalendar.Entities
{
    public class Event
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Recurring { get; set; } = true; // true = toda ano, false = apenas um ano específico
        public int? Year { get; set; } // preenchido apenas se Recurring = false
    }
}
