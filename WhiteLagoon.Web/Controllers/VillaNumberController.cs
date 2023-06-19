using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VillaNumberController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index(int villaId)
        {
            List<VillaNumber> villaList = _context.VillaNumbers.ToList();
            return View(villaList);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(VillaNumber villaNumber)
        {
            if (ModelState.IsValid)
            {
                _context.VillaNumbers.Add(villaNumber);
                _context.SaveChanges();
                TempData["success"] = "Villa Number Successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(villaNumber);
        }
    }
}
