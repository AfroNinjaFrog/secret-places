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
        public async Task<IActionResult> Index(string TitleSearchString, string DateSearch, int PlaceSearch)
        {
            var reviews = from d in _context.Review
                        select d;

            foreach (var currentReview in reviews)
            {
                currentReview.Place = _context.Place.First(c => c.ID == currentReview.PlaceID);
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

            if (PlaceSearch != -1 && PlaceSearch != 0)
            {
                reviews = reviews.Where(p => p.PlaceID == PlaceSearch);
            }

            PopulatePlacesSearchList();

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
            PopulatePlacesDropDownList();
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Content,Title,PlaceID")] Review review)
        {
            var loggedUser = await _manager.GetUserAsync(User);

            if (loggedUser == null)
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid) return View(review);
            review.PublishDate = DateTime.Now;
            review.Place = (Place)(from d in _context.Place
                where d.ID == review.PlaceID
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
            PopulatePlacesDropDownList(review.PlaceID);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Content,Title,PublishDate,PlaceID")] Review review)
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
                    review.Place = (Place)(from d in _context.Place
                                             where d.ID == review.PlaceID
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
            PopulatePlacesDropDownList(review.PlaceID);
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
                .Include(p => p.Place)
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

        private void PopulatePlacesDropDownList(object selectedPlace = null)
        {
            var PlaceQuery = from d in _context.Place
                                   orderby d.Name
                                   select d;

            ViewBag.PlaceID = new SelectList(PlaceQuery, "ID", "Name", selectedPlace);
        }

        private void PopulatePlacesSearchList()
        {
            var PlacesQuery = from d in _context.Place
                                 orderby d.Name
                                 select d;

            ViewBag.PlaceID = new SelectList(PlacesQuery, "ID", "Name", null);
        }

        // GET: Reviews/GroupByPlace
        public async Task<ActionResult> Graphs()
        {
            var query = from review in _context.Review
                        group review by review.PlaceID into p
                        join Place in _context.Place on p.Key equals Place.ID
                        select new GroupByPlace() { PlaceName = Place.Name, TotalReviews = p.Sum(s => 1) };

            return View(await query.OrderByDescending(p => p.TotalReviews).ToListAsync());
        }

        [HttpGet]
        public ActionResult GetGroupByPlace()
        {
            var query = from review in _context.Review
                        group review by review.Place.Name into g
                        select new GroupByPlace() { PlaceName = g.Key, TotalReviews = g.Sum(p => 1) };

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

        public IActionResult RecommendedPlaces()
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
                review.Place = _context.Place.First(c => c.ID == review.PlaceID);
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
		
        // Create a svm node array, each node has its value of how many times does a word from the vocabulary
        // appears in the review
        private static svm_node[] CreateNode(string x, IReadOnlyList<string> vocabulary)
        {
            // Creates a list the size of the vocabulary 
            var node = new List<svm_node>(vocabulary.Count);

            // Creates an array from the review's words
            var words = x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // For each word in the vocabulary
            for (var i = 0; i < vocabulary.Count; i++)
            {
                // Checks how many times does a word appears in the vocabulary
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