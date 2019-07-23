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
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        public RestaurantsController(ApplicationDbContext context, UserManager<ApplicationUser> manager)
        {
            _context = context;
            _manager = manager;

        }

        // GET: Restaurants
        public async Task<IActionResult> Index(string NameSearch, string RestaurantTypeSearch, string KosherSearch)
        {
            var restaurants = from d in _context.Restaurant
                            select d;

            if (!String.IsNullOrEmpty(NameSearch))
            {
                restaurants = restaurants.Where(p => p.Name.Contains(NameSearch));
            }

            if (!String.IsNullOrEmpty(RestaurantTypeSearch) && !RestaurantTypeSearch.Equals("All"))
            {
                var restaurantType = (RestaurantType)Convert.ToInt32(RestaurantTypeSearch);
                restaurants = restaurants.Where(p => p.RestaurantType.Equals(restaurantType));
            }

            if (!String.IsNullOrEmpty(KosherSearch) && !KosherSearch.Equals("All"))
            {
                var isKosher = KosherSearch.Equals("Yes");
                restaurants = restaurants.Where(p => p.IsKosher == isKosher);
            }
            
            return View(await restaurants.ToListAsync());
        }

        // GET: Restaurants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(m => m.ID == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // GET: Restaurants/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Restaurants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,RestaurantType,IsKosher,lon,lat")] Restaurant restaurant)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // GET: Restaurants/Edit/5
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

            var restaurant = await _context.Restaurant.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }
            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,RestaurantType,IsKosher,lon,lat")] Restaurant restaurant)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index");
            }

            if (id != restaurant.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantExists(restaurant.ID))
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
            return View(restaurant);
        }

        // GET: Restaurants/Delete/5
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

            var restaurant = await _context.Restaurant
                .FirstOrDefaultAsync(m => m.ID == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // POST: Restaurants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index");
            }

            var restaurant = await _context.Restaurant.FindAsync(id);
            _context.Restaurant.Remove(restaurant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RestaurantExists(int id)
        {
            return _context.Restaurant.Any(e => e.ID == id);
        }
    }
}
