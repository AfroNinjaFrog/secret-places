using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DataAccess;
using System.Configuration;
using SecretPlaces.Data;
using SecretPlaces.Models;
using libsvm;

namespace SecretPlaces.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;

        public ReviewsController(ApplicationDbContext context, UserManager<ApplicationUser> manager)
        {
            _context = context;
            _manager = manager;
        }

        // GET: Reviews
        public async Task<IActionResult> Index(string TitleSearchString, string DateSearch, int RestaurantSearch)
        {
            var reviews = from d in _context.Review
                        select d;

            foreach (var currentReview in reviews)
            {
                currentReview.Restaurant = _context.Restaurant.First(c => c.ID == currentReview.RestaurantID);
                currentReview.Comments = _context.Comment.Where(c => c.ReviewID == currentReview.ID).ToList();
            }

            if (!string.IsNullOrEmpty(TitleSearchString))
            {
                reviews = reviews.Where(p => p.Title.Contains(TitleSearchString));
            }

            if (!string.IsNullOrEmpty(DateSearch))
            {
                reviews = reviews.Where(p => p.PublishDate.Date == DateTime.Parse(DateSearch).Date);
            }

            if (RestaurantSearch != -1 && RestaurantSearch != 0)
            {
                reviews = reviews.Where(p => p.RestaurantID == RestaurantSearch);
            }

            PopulateRestaurantsSearchList();

            return View(await reviews.OrderByDescending(p => p.PublishDate).ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .FirstOrDefaultAsync(m => m.ID == id);
            if (review == null)
            {
                return NotFound();
            }
            
            return View(review);
        }

        // GET: Reviews/Create
        public IActionResult Create()
        {
            PopulateRestaurantsDropDownList();
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Content,Title,RestaurantID")] Review review)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null)
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid) return View(review);
            review.PublishDate = DateTime.Now;
            review.Restaurant = (Restaurant)(from d in _context.Restaurant
                where d.ID == review.RestaurantID
                select d).First();

            review.UploaderUsername = HttpContext.User.Identity.Name;

            _context.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public ActionResult PostComment(int ReviewId, string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var comment = new Comment
                {
                    Content = content,
                    ReviewID = ReviewId,
                    CreationDate = DateTime.Now,
                    UploaderUsername = HttpContext.User.Identity.Name
                };

                _context.Comment.Add(comment);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteComment(int id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null)
            {
                return RedirectToAction("Index");
            }

            var comment = await _context.Comment.FindAsync(id);

            if (comment.UploaderUsername != loggedUser.UserName)
            {
                return RedirectToAction("Index");
            }

            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null)
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            PopulateRestaurantsDropDownList(review.RestaurantID);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Content,Title,PublishDate,RestaurantID")] Review review)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null)
            {
                return RedirectToAction("Index");
            }

            if (id != review.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    review.PublishDate = DateTime.Now;
                    review.Restaurant = (Restaurant)(from d in _context.Restaurant
                                             where d.ID == review.RestaurantID
                                             select d).First();
                    review.UploaderUsername = HttpContext.User.Identity.Name;
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ID))
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
            PopulateRestaurantsDropDownList(review.RestaurantID);
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null)
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(p => p.Restaurant)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (review == null)
            {
                return NotFound();
            }

            if (review.UploaderUsername != loggedUser.UserName)
            {
                return RedirectToAction("Index");
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null)
            {
                return RedirectToAction("Index");
            }

            var review = await _context.Review.FindAsync(id);
            
            _context.Review.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Review.Any(e => e.ID == id);
        }

        private void PopulateRestaurantsDropDownList(object selectedRestaurant = null)
        {
            var RestaurantQuery = from d in _context.Restaurant
                                   orderby d.Name
                                   select d;

            ViewBag.RestaurantID = new SelectList(RestaurantQuery, "ID", "Name", selectedRestaurant);
        }

        private void PopulateRestaurantsSearchList()
        {
            var restaurantsQuery = from d in _context.Restaurant
                                 orderby d.Name
                                 select d;

            ViewBag.RestaurantID = new SelectList(restaurantsQuery, "ID", "Name", null);
        }

        // GET: Reviews/GroupByRestaurant
        public async Task<ActionResult> Graphs()
        {
            var query = from review in _context.Review
                        group review by review.RestaurantID into p
                        join restaurant in _context.Restaurant on p.Key equals restaurant.ID
                        select new GroupByRestaurant() { RestaurantName = restaurant.Name, TotalReviews = p.Sum(s => 1) };

            return View(await query.OrderByDescending(p => p.TotalReviews).ToListAsync());
        }

        [HttpGet]
        public ActionResult GetGroupByRestaurant()
        {
            var query = from review in _context.Review
                        group review by review.Restaurant.Name into g
                        select new GroupByRestaurant() { RestaurantName = g.Key, TotalReviews = g.Sum(p => 1) };

            return Json(query);
        }

        [HttpGet]
        public ActionResult GetCommentsGroupByReview()
        {
            var query = from comment in _context.Comment
                        group comment by comment.ReviewID into c
                        join review in _context.Review on c.Key equals review.ID
                        select new { Name = review.Title, Count = c.Sum(p => 1) };

            return Json(query.OrderByDescending(p => p.Count));
        }

        public IActionResult RecommendedRestaurants()
        {
            // Load the predifined data for smv algorithm
            var dataFilePath = "./wwwroot/svm/words.csv";
            var dataTable = DataTable.New.ReadCsv(dataFilePath);
            var data = dataTable.Rows.Select(row => row["Text"]).ToList();

            // Load classes (-1 or +1)
            var classes = dataTable.Rows.Select(row => double.Parse(row["IsRecommended"]))
                                       .ToArray();

            // Get words
            var vocabulary = data.SelectMany(GetWords).Distinct().OrderBy(word => word).ToList();

            // Generate a svm problem
            var problem = CreateProblem(data, classes, vocabulary.ToList());

            // Create and train a smv model
            const int C = 1;
            var model = new libsvm.C_SVC(problem, KernelHelper.LinearKernel(), C);

            var _predictionDictionary = new Dictionary<int, string> { { -1, "NotRecommended" }, { 1, "Recommended" } };

            // Get all reviews
            var reviews = _context.Review.ToList();

            // Get recommended reviews
            foreach (var review in reviews)
            {
                if (review.Content != null)
                {
                    var node = CreateNode(review.Content, vocabulary);
                    var prediction = model.Predict(node);

                    review.IsRecommended = _predictionDictionary[(int)prediction] == "Recommended";
                }
                else
                {
                    review.IsRecommended = false;
                }
            }

            var recommendedReviews = reviews.Where(p => p.IsRecommended == true);

            foreach (var review in recommendedReviews)
            {
                review.Restaurant = _context.Restaurant.First(c => c.ID == review.RestaurantID);
                review.Comments = _context.Comment.Where(c => c.ReviewID == review.ID).ToList();
            }

            return View(recommendedReviews.OrderByDescending(p => p.PublishDate));
        }
		

        private static IEnumerable<string> GetWords(string x)
        {
            return x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private svm_problem CreateProblem(IEnumerable<string> x, double[] y, IReadOnlyList<string> vocabulary)
        {
            return new svm_problem
            {
                y = y,
                x = x.Select(xVector => CreateNode(xVector, vocabulary)).ToArray(),
                l = y.Length
            };
        }
		

        private static svm_node[] CreateNode(string x, IReadOnlyList<string> vocabulary)
        {
            var node = new List<svm_node>(vocabulary.Count);

            var words = x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < vocabulary.Count; i++)
            {
                var occurenceCount = words.Count(s => string.Equals(s, vocabulary[i], StringComparison.OrdinalIgnoreCase));
                if (occurenceCount == 0)
                    continue;

                node.Add(new svm_node
                {
                    index = i + 1,
                    value = occurenceCount
                });
            }

            return node.ToArray();
        }
    }
}