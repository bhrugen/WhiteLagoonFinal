using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly List<string> _bookedStatus = new List<string> { "Approved", "CheckedIn" };
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize]
        //INSTALL JSDELIVER add client side and search for jquery-ajax-unobtrusive@3.2.6
        //show if only admin were allowed then this will not work and go to access denided
        //[Authorize(Roles =SD.Role_Admin)]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            Booking booking = new()
            {
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
            };
            return View(booking);
        }
    }
}
