using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebCalendar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public EventController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("save")]
        public IActionResult SaveEvent([FromBody] EventData eventData)
        {
            // Validações
            if (eventData == null || eventData.day < 1 || eventData.day > 31 || eventData.month < 1 || eventData.month > 12 ||
                string.IsNullOrWhiteSpace(eventData.type))
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            string type = eventData.type.Trim().ToLower();
            string name = (eventData.name ?? "").Trim();
            int day = eventData.day;
            int month = eventData.month;

            try
            {
                if (type == "birthday")
                {
                    var path = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "birthday.json");
                    var birthdays = LoadBirthdays(path);

                    // Encontra aniversário para este dia/mês
                    var existing = birthdays.FirstOrDefault(b => b.day == day && b.month == month);

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        // Se nome está vazio, remove a entrada
                        if (existing != null)
                        {
                            birthdays.Remove(existing);
                        }
                    }
                    else
                    {
                        // Se tem nome, adiciona ou atualiza
                        if (existing != null)
                        {
                            existing.name = name;
                        }
                        else
                        {
                            birthdays.Add(new EventInfo { day = day, month = month, name = name });
                        }
                    }

                    var json = JsonSerializer.Serialize(birthdays, new JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(path, json);
                }
                else if (type == "holiday")
                {
                    var path = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "holidays.json");
                    var holidays = LoadHolidays(path);

                    // Encontra feriado para este dia/mês
                    var existing = holidays.FirstOrDefault(h => h.day == day && h.month == month);

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        // Se nome está vazio, remove a entrada
                        if (existing != null)
                        {
                            holidays.Remove(existing);
                        }
                    }
                    else
                    {
                        // Se tem nome, adiciona ou atualiza
                        if (existing != null)
                        {
                            existing.name = name;
                        }
                        else
                        {
                            holidays.Add(new EventInfo { day = day, month = month, name = name });
                        }
                    }

                    var json = JsonSerializer.Serialize(holidays, new JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(path, json);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Invalid event type" });
                }

                return Ok(new { success = true, message = "Saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private List<EventInfo> LoadBirthdays(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    var birthdays = JsonSerializer.Deserialize<List<EventInfo>>(json);
                    return birthdays ?? new List<EventInfo>();
                }
                return new List<EventInfo>();
            }
            catch
            {
                return new List<EventInfo>();
            }
        }

        private List<EventInfo> LoadHolidays(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    var holidays = JsonSerializer.Deserialize<List<EventInfo>>(json);
                    return holidays ?? new List<EventInfo>();
                }
                return new List<EventInfo>();
            }
            catch
            {
                return new List<EventInfo>();
            }
        }

        public class EventData
        {
            public int day { get; set; }
            public int month { get; set; }
            public string type { get; set; }
            public string name { get; set; }
        }

        public class EventInfo
        {
            public int day { get; set; }
            public int month { get; set; }
            public string name { get; set; }
        }
    }
}
