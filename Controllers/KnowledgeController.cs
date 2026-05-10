using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CareNote.Models;
using CareNote.Services;
using System.Security.Claims;

// Denna Controller hanterar kunskapsfiler för användaren

namespace CareNote.Controllers
{
    [Authorize]
    public class KnowledgeController : Controller
    {
        private readonly IKnowledgeService _knowledgeService;
        private readonly ILogger<KnowledgeController> _logger;

        public KnowledgeController(IKnowledgeService knowledgeService, ILogger<KnowledgeController> logger)
        {
            _knowledgeService = knowledgeService;
            _logger = logger;
        }

        private string GetEffectiveUserId()
        {
            var userId = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId)) return userId;

            userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId)) return userId;

            userId = User?.FindFirst(ClaimTypes.Email)?.Value;
            return userId ?? "anonymous";
        }

        public async Task<IActionResult> Index() // Index: listar användarens filer
        {
            var userId = GetEffectiveUserId();
            _logger.LogDebug("KnowledgeController.Index called for user: {UserId}", userId);

            var files = await _knowledgeService.GetUserFilesAsync(userId);
            files ??= new List<KnowledgeFile>();
            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file) // UploadFile: laddar upp och indexerar filer (PDF, Word, txt)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Välj en fil att ladda upp.";
                return RedirectToAction(nameof(Index));
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                TempData["Error"] = "Filen är för stor. Maximal storlek är 10MB.";
                return RedirectToAction(nameof(Index));
            }

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["Error"] = "Endast PDF, Word och textfiler är tillåtna.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await using var stream = file.OpenReadStream();
                await _knowledgeService.ProcessFileAsync(stream, file.FileName, GetEffectiveUserId());
                TempData["Success"] = $"Fil '{file.FileName}' uppladdad och indexerad!";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "UploadFile failed for {FileName}", file.FileName);
                TempData["Error"] = "Kunde inte ladda upp filen. Försök igen.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Search(string query) // Search: söker i användarfiler och externa källor
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = false, error = "Ange ett sökord." });
            }

            if (query.Length < 2)
            {
                return Json(new { success = false, error = "Sökordet måste vara minst 2 tecken." });
            }

            try
            {
                _logger.LogInformation("Search initiated for query: {Query}", query);

                // Search user files
                var userResults = await _knowledgeService.SearchAsync(query, GetEffectiveUserId());

                // Search external resources
                var externalResults = await _knowledgeService.SearchExternalSourcesAsync(query);

                return Json(new
                {
                    success = true,
                    userResults,
                    externalResults,
                    searchInfo = new {
                        query = query,
                        userResultsCount = userResults?.Count ?? 0,
                        externalResultsCount = externalResults?.Count ?? 0
                    }
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Search failed for query: {Query}", query);
                return Json(new { 
                    success = false, 
                    error = "Sökningen kunde inte slutföras. Kontrollera din internetanslutning och försök igen." 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(int fileId) // DeleteFile: tar bort en fil

        {
            try
            {
                await _knowledgeService.DeleteFileAsync(fileId, GetEffectiveUserId());
                return Json(new { success = true, message = "Filen har tagits bort." });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "DeleteFile failed for fileId: {FileId}", fileId);
                return Json(new { success = false, error = "Kunde inte ta bort filen." });
            }
        }
    }
}