using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);

            Booking booking = new()
            {
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name,
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {

            var villa = _unitOfWork.Villa.Get(u => u.Id == booking.VillaId);

            booking.TotalCost = (villa.Price * booking.Nights);
            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();
            return RedirectToAction(nameof(BookingConfirmation), new { bookingId = booking.Id });
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            return View(bookingId);
        }
    }
}
