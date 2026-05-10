using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CareNote.Models;
using Microsoft.AspNetCore.Authorization;
using CareNote.Services;

// Detta är huvudcontroller för hemsidan

namespace CareNote.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GroqAIService _aiService;

        public HomeController(ILogger<HomeController> logger, GroqAIService aiService)
        {
            _logger = logger;
            _aiService = aiService;
        }

        // Landing-sidan ska vara tillgänglig utan inloggning
        [AllowAnonymous]
        public IActionResult Landing()
        {
            return View();
        }

        // Dashboard kräver inloggning (tack vare [Authorize] på klassen)
        public IActionResult Index()
        {
            return View();
        }

        // Settings kräver inloggning
        public IActionResult Privacy()
        {
            return View();
        }

        // ✅ NY: Reflection Mode
        public IActionResult Reflection()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Reflect([FromBody] ReflectionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return Json(new { success = false, error = "Meddelande krävs." });
                }

                //  MEMORY: Begränsa meddelandelängd
                if (request.Message.Length > 1000)
                {
                    request.Message = request.Message.Substring(0, 1000) + "... [trunkerad]";
                }

                //  Använd AI-tjänsten för reflektionssvar
                var prompt = $@"Du är en reflektionscoach för sjuksköterskor. Användaren vill reflektera över: '{request.Message}'

VIKTIGA REGLER:
1. Var empatisk och professionell
2. Ställ öppna, reflekterande frågor
3. Fokusera på lärande och utveckling
4. Håll svaren korta och engagerande (max 2 meningar)
5. Använd svenska

Exempel på bra svar:
- 'Vad tror du oron berodde på i den situationen?'
- 'Hur upplevde du kommunikationen med patienten?'
- 'Vad lärde du dig från den här erfarenheten?'
- 'Hur skulle du hantera en liknande situation framöver?'

Ge ENDAST reflektionsfrågan/svaret utan extra kommentarer.";

                var aiResponse = await _aiService.GenerateResponseAsync(prompt);
                
                _logger.LogInformation("Reflection completed for user: {User}", User.Identity.Name);
                
                return Json(new { success = true, response = aiResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reflection mode for user: {User}", User.Identity.Name);
                return Json(new { success = false, error = "Reflektion misslyckades. Försök igen." });
            }
        }

        // Error-sidan ska vara tillgänglig för alla
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ReflectionRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}