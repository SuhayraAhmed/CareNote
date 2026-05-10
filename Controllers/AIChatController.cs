using Microsoft.AspNetCore.Mvc;
using CareNote.Services;
using Microsoft.AspNetCore.Authorization; 

// Denna Controller är för AI-chatt 
// Använder GroqAIService för att generera AI-svar
// Deep Seek AI användes för att implementera AI-chatten

namespace CareNote.Controllers
{
    [Authorize] 
    public class AIChatController : Controller
    {
        private readonly GroqAIService _aiService;

        public AIChatController(GroqAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request) // SendMessage tar emot meddelanden
                                                                                     // från frontend och returnerar AI-svar
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Message cannot be empty." });

            var response = await _aiService.GenerateResponseAsync(request.Message);
            return Ok(new { reply = response });
        }

        public IActionResult Index() // Index visar chatt-sidan
        {
            return View();
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}