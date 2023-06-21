using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Utility
{
    public static  class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static int VillaRoomsAvailable_Count(Villa villa, List<VillaNumber> villaNumberList, DateOnly checkInDate, int nights,
            List<Booking> bookings)

        {
            List<int> bookingInDate = new();

            var roomsInVilla = villaNumberList.Where(m => m.VillaId == villa.Id);
            for (int i = 0; i < nights; i++)
            {
                //we need to ignore checkout date because at the time room will be available
                var villasBooked = bookings.Where(m => m.CheckInDate <= checkInDate.AddDays(i) && m.CheckOutDate > checkInDate.AddDays(i) &&
                                      m.VillaId == villa.Id).ToList();

                foreach (var booking in villasBooked)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        //we will add booking Id that needs a room in this date range
                        bookingInDate.Add(booking.Id);
                    }
                }

                var totalAvailableRooms = roomsInVilla.Count() - bookingInDate.Count;
                if (totalAvailableRooms > 0)
                {
                    return totalAvailableRooms;
                }
                return 0;
            }
            return 0;

        }
    }
}
