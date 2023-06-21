using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
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


            //it is a regular customer account and we need to capture payment
            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/finalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100), // $20.50 => 2050
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Images = new List<string>()
                        //        {
                        //            Request.Scheme + "://" + Request.Host.Value + villa.ImageUrl.Replace('\\','/')
                        //        },

                    }

                },
                Quantity = 1
            });


            var service = new SessionService();

            Session session = service.Create(options);
            _unitOfWork.Booking.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if (bookingFromDb.Status == SD.StatusPending)
            {
                //this is a pending order

                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingId, session.Id, session.PaymentIntentId);
                    _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusApproved, 0);
                    _unitOfWork.Save();
                }

            }
            return View(bookingId);
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }


        #region API Calls
        [HttpGet]
        public IActionResult GetAll(string status = "")
        {
            IEnumerable<Booking> objBookings;


            if (User.IsInRole(SD.Role_Admin))
            {
                objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa").ToList();
            }
            else
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objBookings = _unitOfWork.Booking
                    .GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }

            //if (!string.IsNullOrWhiteSpace(status))
            //{
            //    objBookings = objBookings.Where(u => u.Status.ToLower() == status.ToLower());
            //}

            return Json(new { data = objBookings });
        }
        #endregion
    }
}
