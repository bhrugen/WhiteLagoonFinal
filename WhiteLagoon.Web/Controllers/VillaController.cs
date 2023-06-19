using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VillaController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            IEnumerable<Villa> villaList = _context.Villas;
            return View(villaList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if (obj.Name == obj.Description?.ToString())
            {
                ModelState.AddModelError("name", "The description cannot exactly match the Name.");
            }
            if (ModelState.IsValid)
            {
                _context.Villas.Add(obj);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Update(int villaId)
        {
            Villa? obj = _context.Villas.FirstOrDefault(x => x.Id == villaId);
            //will retrieve first record if more than one records are found. If no records are found it will return null
            //Villa obj1 = _context.Villas.Where(x => x.Id == villaId).FirstOrDefault();

            // will throw exception if no records are found
            //https://localhost:7049/Villa/Update?villaId=7
            //Villa obj2 = _context.Villas.First(x => x.Id == villaId);

            // will throw exception if more than one records are found
            //Villa obj3 = _context.Villas.SingleOrDefault(x => x.Name.ToLower().Contains("villa11"));

            // will throw exception if more than one records are found or if no records are found
            //Villa obj4 = _context.Villas.Single(x => x.Name.ToLower().Contains("villa11"));
            return View(obj);
        }
    }
}
