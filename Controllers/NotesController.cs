using Microsoft.AspNetCore.Mvc;
using CareNote.Models;
using Microsoft.AspNetCore.Authorization;

namespace CareNote.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private static List<Note> _notes = new();
        private static int _nextId = 1;

        public IActionResult Index()
        {
            return View(_notes.OrderByDescending(n => n.Id));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Note note)
        {
            if (!ModelState.IsValid)
                return View(note);

            note.Id = _nextId++;
            note.CreatedAt = DateTime.Now; // ← VIKTIGT
            _notes.Add(note);

            TempData["Success"] = "Anteckningen sparades!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var note = _notes.FirstOrDefault(n => n.Id == id);
            if (note != null)
            {
                _notes.Remove(note);
                TempData["Success"] = "Anteckningen raderades!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}