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
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> manager)
        {
            _context = context;
            _manager = manager;

        }

        // GET: Usesrs
        public async Task<IActionResult> Index(string UserNameSearch, string AdminSearch)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            var applicationUsers = await _manager.Users.ToListAsync<ApplicationUser>();

            if (!String.IsNullOrEmpty(UserNameSearch))
            {
                applicationUsers = applicationUsers.Where(u => u.UserName.Contains(UserNameSearch)).ToList();
            }

            if (!String.IsNullOrEmpty(AdminSearch) && !AdminSearch.Equals("All"))
            {
                var isAdmin = AdminSearch.Equals("Yes");
                applicationUsers = applicationUsers.Where(u => u.IsAdmin == isAdmin).ToList();
            }

            List<User> UsersList = new List<User>();

            foreach (var user in applicationUsers)
            {
                User UserModel = new User
                {
                    ID = user.Id,
                    Username = user.UserName,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    IsAdmin = user.IsAdmin
                };
                UsersList.Add(UserModel);
            }

            return View(UsersList);
        }



        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _manager.Users.FirstAsync<ApplicationUser>(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(new User
            {
                ID = user.Id,
                Username = user.UserName,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                IsAdmin = user.IsAdmin
            });
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ID,Username,Firstname,Lastname,IsAdmin")] User user)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != user.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ApplicationUser oldUser = await _manager.Users.FirstAsync<ApplicationUser>(u => u.Id == id);
                    oldUser.UserName = user.Username;
                    oldUser.Email = user.Username;
                    oldUser.Firstname = user.Firstname;
                    oldUser.Lastname = user.Lastname;
                    
                    if (oldUser.UserName != loggedUser.UserName)
                    {
                        oldUser.IsAdmin = user.IsAdmin;
                    }

                    IdentityResult result = await _manager.UpdateAsync(oldUser);

                    if (!result.Succeeded)
                    {

                        throw new DbUpdateConcurrencyException("Save failed", null);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.ID))
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

            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            if (String.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            if (id == _manager.GetUserId(User))
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _manager.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(new User
            {
                ID = user.Id,
                Username = user.UserName,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                IsAdmin = user.IsAdmin
            });
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null || !loggedUser.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _manager.Users
               .FirstOrDefaultAsync(m => m.Id == id);
            await _manager.DeleteAsync(user);
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _manager.Users.Any<ApplicationUser>(e => e.Id == id);
        }

        public IActionResult Graphs()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetReviewsGroupByUser()
        {
            var query = from review in _context.Review
                        group review by review.UploaderUsername into g
                        select new { Name = g.Key, Count = g.Sum(p => 1) };

            return Json(query.OrderByDescending(u => u.Count));
        }

        [HttpGet]
        public ActionResult GetCommentsGroupByUser()
        {
            var query = from comment in _context.Comment
                        group comment by comment.UploaderUsername into g
                        select new { Name = g.Key, Count = g.Sum(p => 1) };

            return Json(query.OrderByDescending(u => u.Count));
        }
    }
}