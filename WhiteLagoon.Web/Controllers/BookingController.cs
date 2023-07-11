using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using System.Globalization;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using System.Reflection.Metadata;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly List<string> _bookedStatus = new List<string> { "Approved", "CheckedIn" };
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
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


            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();

            int roomsAvailable = SD.VillaRoomsAvailable_Count(villa, villaNumbersList,
                booking.CheckInDate, booking.Nights, bookedVillas);
            if (roomsAvailable == 0)
            {
                TempData["error"] = "Room has been sold out!";
                //no rooms available
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = booking.VillaId,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }


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
        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumbers = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId, bookingFromDb.CheckInDate);

                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId
                            && availableVillaNumbers.Any(x => x == m.Villa_Number)).ToList();
            }
            else
            {
                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId && m.Villa_Number == bookingFromDb.VillaNumber).ToList();
            }
            return View(bookingFromDb);
        }

        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn,booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, 0);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult GeneratePDF(int id)
        {
            string basePath = _webHostEnvironment.WebRootPath;

            // Create a new document
            WordDocument doc = new();

            // Load the template.
            string dataPathSales = basePath + @"/exports/SampleVilla.docx";
            FileStream fileStream = new FileStream(dataPathSales, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            doc.Open(fileStream, FormatType.Automatic);



            //Get Villa Booking Details
            Booking bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == id, includeProperties: "User,Villa");

            
            TextSelection textSelection = doc.Find("xx_customer_name", false, true);
            WTextRange textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Name;
            
            textSelection = doc.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Phone;

            textSelection = doc.Find("xx_customer_email", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Email;

            textSelection = doc.Find("xx_Booking_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING DATE - " + bookingFromDb.BookingDate.ToShortDateString();

            textSelection = doc.Find("xx_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING ID: " + bookingFromDb.Id.ToString();

            textSelection = doc.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.PaymentDate.ToShortDateString();

            textSelection = doc.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckInDate.ToShortDateString();

            textSelection = doc.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckOutDate.ToShortDateString();

            textSelection = doc.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.TotalCost.ToString("c");


            //Sets highlight color
            //textRange.CharacterFormat.HighlightColor = Color.Yellow;


            //MailMergeDataTable villaBookingDetailDataSource = new MailMergeDataTable("VillaBookingDetails", villaBookingDetail.VillaBookingDetails);
            //doc.MailMerge.ExecuteGroup(villaBookingDetailDataSource);

            ////Get Villa Details
            //MailMergeDataTable villaDetailsDataSource = new MailMergeDataTable("VillaDetails", villaBookingDetail.VillaDetails);
            //doc.MailMerge.ExecuteGroup(villaDetailsDataSource);

            ////Get Villa Payment Details
            //MailMergeDataTable villaPaymentDetailsDataSource = new MailMergeDataTable("VillaPaymentDetails", villaBookingDetail.VillaPaymentDetails);
            //doc.MailMerge.ExecuteGroup(villaPaymentDetailsDataSource);

            //// Using Merge events to do conditional formatting during runtime.
            //doc.MailMerge.MergeField += new MergeFieldEventHandler(MailMerge_MergeField);

            using (DocIORenderer render = new DocIORenderer())
            {
                //Converts Word document into PDF document
                PdfDocument pdfDocument = render.ConvertToPDF(doc);

                //Saves the PDF document to MemoryStream.
                MemoryStream stream = new MemoryStream();
                pdfDocument.Save(stream);
                stream.Position = 0;

                //Download PDF document in the browser.
                return File(stream, "application/pdf", "VillaDetails.pdf");
            }
        }

       
        private void MailMerge_MergeField(object sender, MergeFieldEventArgs args)
        {
            // Conditionally format data during Merge.
            if (args.RowIndex % 2 == 0)
            {
                args.CharacterFormat.TextColor = Syncfusion.Drawing.Color.DarkBlue;
            }

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

            if (!string.IsNullOrWhiteSpace(status))
            {
                objBookings = objBookings.Where(u => u.Status.ToLower() == status.ToLower());
            }

            return Json(new { data = objBookings });
        }

        public List<int> AssignAvailableVillaNumberByVilla(int villaId, DateOnly checkInDate)
        {
            List<int> availableVillaNumbers = new List<int>();

            var villaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == villaId).ToList();

            var checkedInVilla = _unitOfWork.Booking.GetAll().Where(m => m.Status == SD.StatusCheckedIn && m.VillaId == villaId).Select(u => u.VillaNumber);


            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    //Villa is not checked in
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }
        #endregion
    }
}
