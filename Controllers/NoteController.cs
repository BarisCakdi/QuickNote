using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickNote.Data;
using QuickNote.Models;
using QuickNote.ViewModels;
using System.Security.Claims;

namespace QuickNote.Controllers
{
    [Authorize]
    public class NoteController : Controller
    {

        private readonly ApplicationDbContext _context;

        public NoteController(ApplicationDbContext context, IdentityUser user)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notes = _context.Notes.Where(x => x.UserId == userId && x.Status == Status.Published)
                .Include(x => x.NoteTags)
                .ThenInclude(x => x.Tag)
                .OrderByDescending(x => x.UpdateDate)
                .ToList();

            var uniqueTags = notes
                .SelectMany(x => x.NoteTags)
                .Select(x => x.Tag)
                .GroupBy(x => x.Name)
                .Select(x => x.First())
                .ToList();

            var ViewModel = new HomePage()
            {
                Notes = notes,
                Tags = uniqueTags
            };

            return View(ViewModel);
        }

        [HttpPost]
        public IActionResult CreatNote(Note note, string tags)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            note.UserId = userId;
            note.Status = note.Status;
            note.CreatedDate = DateTime.Now;
            note.UpdateDate = DateTime.Now;
            _context.Notes.Add(note);
            _context.SaveChanges();

            var noteTags = _context.NoteTags.Where(x => x.NoteId == note.Id).ToList();

            if (!string.IsNullOrEmpty(tags))
            {
                var multipleTags = tags.Split(',')
                    .Select(x => x.Trim())
                    .ToList();

                foreach (var tagName in multipleTags)
                {
                    var tag = _context.Tags.FirstOrDefault(x => x.Name == tagName && userId == userId);

                    if (tag == null)
                    {
                        _context.Tags.Add(new Tag { Name = tagName, UserId = userId });
                        _context.SaveChanges();
                    }

                    var noteTag = new NoteTag
                    {
                        NoteId = note.Id,
                        TagId = tag.Id
                    };

                    _context.NoteTags.Add(noteTag);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult UpdateNote(Note model, string tags)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var note = _context.Notes.FirstOrDefault(x => x.Id == model.Id && x.UserId == userId);
            if (note == null)
            {
                return NotFound(new { message = "Note bulunamadı" });
            }

            _context.Notes.Update(note);
            _context.SaveChanges();


            var multipleTags = tags.Split(',')
                .Select(x => x.Trim())
                .ToList();

            var multipleTagRemove = note.NoteTags.Where(x => multipleTags.Contains(x.Tag.Name)).ToList();

            foreach (var tagName in multipleTags)
            {
                var tag = _context.Tags.FirstOrDefault(x => x.Name == tagName && userId == userId);

                if (tag == null)
                {
                    _context.Tags.Add(new Tag { Name = tagName, UserId = userId });
                    _context.SaveChanges();
                }

                var noteTag = new NoteTag
                {
                    NoteId = note.Id,
                    TagId = tag.Id
                };

                _context.NoteTags.Add(noteTag);
                _context.SaveChanges();
            }


            return RedirectToAction("Index");
        }

        public IActionResult ArchivePage(Note note)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notes = _context.Notes.Where(x => x.UserId == userId && x.Status == Status.Archived)
                .Include(x => x.NoteTags)
                .ThenInclude(x => x.Tag)
                .OrderByDescending(x => x.UpdateDate)
                .ToList();

            var uniqueTags = notes
                .SelectMany(x => x.NoteTags)
                .Select(x => x.Tag)
                .GroupBy(x => x.Name)
                .Select(x => x.First())
                .ToList();

            var ViewModel = new HomePage()
            {
                Notes = notes,
                Tags = uniqueTags
            };

            return View(ViewModel);
        }
        [HttpPost]
        public IActionResult DeleteNote(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notes = _context.Notes.FirstOrDefault(x => x.Id == id && x.UserId == userId);

            notes.Status = Status.Deleted;
            _context.Notes.Update(notes);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult ArchiveNote(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notes = _context.Notes.FirstOrDefault(x => x.Id == id && x.UserId == userId);

            notes.Status = Status.Archived;
            _context.Notes.Update(notes);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RecoverNote(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notes = _context.Notes.FirstOrDefault(x => x.Id == id && x.UserId == userId);

            notes.Status = Status.Published;
            _context.Notes.Update(notes);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Search(string searchString)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notes = _context.Notes
                .Where(x => x.UserId == userId && (x.Title.Contains(searchString) || x.Content.Contains(searchString) || x.NoteTags.Any(x => x.Tag.Name.Contains(searchString))))
                .Include(x => x.NoteTags)
                .ThenInclude(x => x.Tag)
                .OrderByDescending(x => x.UpdateDate)
                .ToList();

            var viewModel = new SearchResult()
            {
                SearchString = searchString,
                Notes = notes,
                Tags = _context.Tags.Where(x => x.NoteTags.Any(x => x.Note.UserId == userId)).ToList()
            };

            return View(viewModel);
        }
    }
}
