﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            HomeVM homeVM = new ()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList(),
                CheckInDate= DateOnly.FromDateTime(DateTime.Now),
                Nights = 1
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult Index(HomeVM homeVM)
        {

            homeVM.VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            foreach (var villa in homeVM.VillaList)
            {
                //based on date get availability
                if (villa.Id % 2 == 0)
                {
                    villa.IsAvailable = false;
                }

            }
            return View(homeVM);
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