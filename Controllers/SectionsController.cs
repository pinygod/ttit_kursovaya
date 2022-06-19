#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using kekes.Data;
using kekes.Data.Models;
using kekes.Models;
using Microsoft.AspNetCore.Authorization;

namespace kekes.Controllers
{
    public class SectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SectionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sections
        public async Task<IActionResult> Index()
        {
            var model = new SectionsIndexViewModel
            {
                PopularSections = (await _context.Sections.Include(x => x.Posts).ToListAsync()).OrderByDescending(x => x.Posts.Count).Take(5),
                RecentSections = (await _context.Sections.Include(x => x.Posts).ToListAsync()).OrderByDescending(x => x.Posts.OrderBy(p => p.LastActivity).FirstOrDefault()?.LastActivity).Take(5)
            };
            return View(model);
        }

        public async Task<IActionResult> All()
        {
            return View(await _context.Sections.Include(x => x.Posts).ToListAsync());
        }

        // GET: Sections/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections.Include(x => x.Posts)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (section == null)
            {
                return NotFound();
            }

            var model = new SectionDetailsViewModel
            {
                SectionId = section.Id,
                SectionName = section.Name,
                SectionDescription = section.Description,
                Posts = section.Posts != default ? section.Posts : new List<Post>(),
            };

            return View(model);
        }

        // GET: Sections/Create
        [Authorize(Roles = ApplicationRoles.Administrators)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = ApplicationRoles.Administrators)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var section = new Section
                {
                    Name = model.Name,
                    Description = model.Description,
                    Id = Guid.NewGuid()
                };
                _context.Add(section);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Sections/Delete/5
        [Authorize(Roles = ApplicationRoles.Administrators)]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var section = await _context.Sections
                .FirstOrDefaultAsync(m => m.Id == id);
            if (section == null)
            {
                return NotFound();
            }

            return View(section);
        }

        // POST: Sections/Delete/5
        [Authorize(Roles = ApplicationRoles.Administrators)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var section = await _context.Sections.FindAsync(id);
            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SectionExists(Guid id)
        {
            return _context.Sections.Any(e => e.Id == id);
        }
    }
}
