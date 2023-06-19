using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            List<VillaNumber> villaNumberList = _context.VillaNumbers.ToList();
            return View(villaNumberList);
        }
        public IActionResult Create()
        {
            ViewData["VillaList"] = _context.Villas.ToList().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View();
        }

        [HttpPost]
        public IActionResult Create(VillaNumber villaNumber)
        {
            //Remove some validations
            ModelState.Remove("Villa");
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
