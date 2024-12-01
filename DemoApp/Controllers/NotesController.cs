using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;


        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        // GET: Notes
        public async Task<IActionResult> Index(string searchQuery)
        {
            var ftpService = new PhotoUploadFTP();
            var notes = from n in _context.Notes select n;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                notes = notes.Where(n => n.Title.Contains(searchQuery) || n.Description.Contains(searchQuery));
            }

            ViewData["SearchQuery"] = searchQuery;

            foreach (var note in notes)
            {
                note.ImgName = ftpService.DownloadPhoto(note.ImgName) ?? "";
            }

            return View(notes);
        }

        [Authorize]
        // GET: Notes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .FirstOrDefaultAsync(m => m.Id == id);


            if (note == null)
            {
                return NotFound();
            }

            var ftpService = new PhotoUploadFTP();
            note.ImgName = ftpService.DownloadPhoto(note.ImgName) ?? "";

            return View(note);
        }

        [Authorize]
        // GET: Notes/Create
        public IActionResult Create()
        {
            return View();
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Note note)
        {

            if (note.Img.Length > 10 * 1024 * 1024)
            {
                ViewBag.Error = "File size exceeds the limit.";
                return View();
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(Path.GetExtension(note.Img.FileName).ToLower()))
            {
                ViewBag.Error = "Invalid file type. Only .jpg, .jpeg, and .png are allowed.";
                return View();
            }

            var ftpService = new PhotoUploadFTP();
            string ImgName = ftpService.UploadPhoto(note.Img)??"";


            if (ModelState.IsValid)
            {
                note.ImgName = ImgName;
                _context.Add(note);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        [Authorize]
        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            var ftpService = new PhotoUploadFTP();
            
            note.ImgName = ftpService.DownloadPhoto(note.ImgName)??"";

            return View(note);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, Note note)
        {
            if (id != note.Id)
            {
                return NotFound();
            }

            if (note.Img == null)
            {
                _context.Update(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (note.Img.Length > 10 * 1024 * 1024)
                    {
                        ViewBag.Error = "File size exceeds the limit.";
                        return View();
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    if (!allowedExtensions.Contains(Path.GetExtension(note.Img.FileName).ToLower()))
                    {
                        ViewBag.Error = "Invalid file type. Only .jpg, .jpeg, and .png are allowed.";
                        return View();
                    }

                    var ftpService = new PhotoUploadFTP();
                    note.ImgName = ftpService.UploadPhoto(note.Img) ?? "";
                    _context.Update(note);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(note.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }


        [Authorize]
        // GET: Notes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            var ftp = new PhotoUploadFTP();
            note.ImgName = ftp.DownloadPhoto(note.ImgName);

            return View(note);
        }


        [Authorize]
        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                var ftp = new PhotoUploadFTP();
                ftp.DeletePhoto(note.ImgName);
                _context.Notes.Remove(note);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
    }
}
