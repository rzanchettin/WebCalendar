using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebCalendar.Entities;

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

        /// <summary>
        /// Salva um evento (aniversário, feriado ou nota) em um arquivo JSON correspondente. 
        /// Se o nome do evento estiver vazio, a entrada será removida. Retorna uma resposta indicando sucesso ou falha.
        /// </summary>
        /// <param Name="eventData"></param>
        /// <returns></returns>
        [HttpPost("save")]
        public IActionResult SaveEvent([FromBody] EventData eventData)
        {
            // Validações
            if (eventData == null || eventData.Day < 1 || eventData.Day > 31 || eventData.Month < 1 || eventData.Month > 12 ||
                string.IsNullOrWhiteSpace(eventData.Type))
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            string type = eventData.Type.Trim().ToLower();
            string name = (eventData.Name ?? "").Trim();
            int day = eventData.Day;
            int month = eventData.Month;
            int year = eventData.Year;
            bool recurring = eventData.Recurring;

            try
            {

                string path = string.Empty;
                List<EventInfo> events = [];

                if (type == "birthdays" || type == "holidays" || type == "notes")
                {
                    path = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), $"{type}.json");
                    events = LoadEvents(path);

                    // Encontra evento
                    var existing = events.FirstOrDefault(b => b.Day == day && b.Month == month);

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        // Se nome está vazio, remove a entrada
                        if (existing != null)
                        {
                            events.Remove(existing);
                            existing.Year = year;
                            existing.Recurring = recurring;
                        }
                    }
                    else
                    {
                        // Se tem nome, adiciona ou atualiza
                        if (existing != null)
                        {
                            existing.Name = name;
                        }
                        else
                        {
                            events.Add(new EventInfo { Day = day, Month = month, Name = name, Year = year, Recurring = recurring });
                        }
                    }

                    var json = JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true });
                    System.IO.File.WriteAllText(path, json);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Invalid event Type" });
                }

                return Ok(new { success = true, message = "Saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Carrega os eventos de um arquivo JSON. Se o arquivo não existir ou ocorrer um erro, retorna uma lista vazia.
        /// </summary>
        /// <param Name="path"></param>
        /// <returns></returns>
        private static List<EventInfo> LoadEvents(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    var events = JsonSerializer.Deserialize<List<EventInfo>>(json);
                    return events ?? [];
                }
                return [];
            }
            catch
            {
                return [];
            }
        }
    }
}
