using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            //https://localhost:7052/Villa/Update?villaId=10 this will not work
            return View();
        }
    }
}