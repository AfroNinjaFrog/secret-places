using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecretPlaces.Data;
using SecretPlaces.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace SecretPlaces.Controllers
{
    public class PlacesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        public PlacesController(ApplicationDbContext context, UserManager<ApplicationUser> manager)
        {
            _context = context;
            _manager = manager;

        }

        // GET: Places
        public async Task<IActionResult> Index(string NameSearch, string PlaceTypeSearch, string KosherSearch)
        {
            var Places = from d in _context.Place
                            select d;

            if (!String.IsNullOrEmpty(NameSearch))
            {
                Places = Places.Where(p => p.Name.Contains(NameSearch));
            }

            /*if (!String.IsNullOrEmpty(PlaceTypeSearch) && !PlaceTypeSearch.Equals("All"))
            {
                var PlaceType = (PlaceType)Convert.ToInt32(PlaceTypeSearch);
                Places = Places.Where(p => p.PlaceType.Equals(PlaceType));
            }

            if (!String.IsNullOrEmpty(KosherSearch) && !KosherSearch.Equals("All"))
            {
                var isKosher = KosherSearch.Equals("Yes");
                Places = Places.Where(p => p.IsKosher == isKosher);
            }*/
            
            return View(await Places.ToListAsync());
        }

        // GET: Places/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Place = await _context.Place
                .FirstOrDefaultAsync(m => m.ID == id);
            if (Place == null)
            {
                return NotFound();
            }

            return View(Place);
        }

        // GET: Places/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Places/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,lon,lat")] Place Place)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                _context.Add(Place);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(Place);
        }

        // GET: Places/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var Place = await _context.Place.FindAsync(id);
            if (Place == null)
            {
                return NotFound();
            }
            return View(Place);
        }

        // POST: Places/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,lon,lat")] Place Place)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index");
            }

            if (id != Place.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Place);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlaceExists(Place.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Index");
            }
            return View(Place);
        }

        // GET: Places/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var Place = await _context.Place
                .FirstOrDefaultAsync(m => m.ID == id);
            if (Place == null)
            {
                return NotFound();
            }

            return View(Place);
        }

        // POST: Places/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index");
            }

            var Place = await _context.Place.FindAsync(id);
            _context.Place.Remove(Place);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlaceExists(int id)
        {
            return _context.Place.Any(e => e.ID == id);
        }
    }
}
