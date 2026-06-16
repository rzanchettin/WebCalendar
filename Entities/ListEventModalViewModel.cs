namespace WebCalendar.Entities
{
    public class ListEventModalViewModel
    {
        public string ModalId { get; set; } = string.Empty;
        public string ModalIdLabel => ModalId + "Label";
        public string Title { get; set; } = string.Empty;
        public string EmptyMessage { get; set; } = string.Empty;
        public int Year { get; set; }
        public List<EventItem> Items { get; set; } = [];
    }
}
