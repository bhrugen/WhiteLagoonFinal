using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Domain.SharedModels;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _db;
        readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DashboardRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RadialBarChartVM> GetBookingsChartDataAsync()
        {
            int totalBooking = await _db.Bookings.CountAsync();
            var countByCurrentMonth = _db.Bookings.Count(r => r.BookingDate >= currentMonthStartDate && r.BookingDate < DateTime.Now);
            var countByPreviousMonth = _db.Bookings.Count(r => r.BookingDate >= previousMonthStartDate && r.BookingDate < currentMonthStartDate);
           return GetRadialChartDataModel(totalBooking, countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartVM> GetRevenueChartDataAsync()
        {
                decimal totalCost = Convert.ToDecimal(await _db.Bookings.SumAsync(x => x.TotalCost));
                var sumByCurrentMonth = _db.Bookings.Where((r => r.BookingDate >= currentMonthStartDate && r.BookingDate < DateTime.Now)).Sum(x => x.TotalCost);
                var sumByPreviousMonth = _db.Bookings.Where(r => r.BookingDate >= previousMonthStartDate && r.BookingDate < currentMonthStartDate).Sum(x => x.TotalCost);
               return GetRadialChartDataModel(totalCost, sumByCurrentMonth, sumByPreviousMonth);
        }
        public async Task<RadialBarChartVM> GetRegisteredUserChartDataAsync()
        {
            int totalCount = await _db.Users.CountAsync();
            var countByCurrentMonth = _db.Users.Count(r => r.CreatedAt >= currentMonthStartDate && r.CreatedAt < DateTime.Now);
            var countByPreviousMonth = _db.Users.Count(r => r.CreatedAt >= previousMonthStartDate && r.CreatedAt < currentMonthStartDate);
            return GetRadialChartDataModel(totalCount, countByCurrentMonth, countByPreviousMonth);
        }

        private static RadialBarChartVM GetRadialChartDataModel(decimal total, double currentMonthCount, double prevMonthCount)
        {
            RadialBarChartVM dashboardRadialBarChartVM = new();
            decimal increaseDecreaseRatio = 100;
            bool isIncrease = true;
            // Considering any non-zero count in current month as 100% increase.

            if (prevMonthCount != 0)
            {
                increaseDecreaseRatio = Convert.ToDecimal(Math.Round(((double)currentMonthCount - prevMonthCount) / prevMonthCount * 100, 2));
                isIncrease = currentMonthCount > prevMonthCount;
            }

            dashboardRadialBarChartVM.TotalCount = total;
            dashboardRadialBarChartVM.IncreaseDecreaseAmount = (decimal)currentMonthCount;
            dashboardRadialBarChartVM.IncreaseDecreaseRatio = increaseDecreaseRatio;
            dashboardRadialBarChartVM.HasRatioIncreased = isIncrease;
            dashboardRadialBarChartVM.Series = new decimal[] { increaseDecreaseRatio };
            return dashboardRadialBarChartVM;
        }

       

        public async Task<DashboardLineChartVM> GetMemberAndBookingChartDataAsync()
        {
            DashboardLineChartVM dashboardLineChartVM = new();
            try
            {
                // Query for new bookings and new customers
                var bookingData = _db.Bookings
                    .Where(b => b.BookingDate.Date >= DateTime.Now.AddDays(-30) && b.BookingDate.Date <= DateTime.Now)
                    .GroupBy(b => b.BookingDate.Date)
                    .Select(g => new
                    {
                        DateTime = g.Key,
                        NewBookingCount = g.Count()
                    })
                    .ToList();

                var customerData = _db.Users
                    .Where(u => u.CreatedAt.Date >= DateTime.Now.AddDays(-30) && u.CreatedAt.Date <= DateTime.Now)
                    .GroupBy(u => u.CreatedAt.Date)
                    .Select(g => new
                    {
                        DateTime = g.Key,
                        NewCustomerCount = g.Count()
                    })
                    .ToList();

                // Perform a left outer join
                var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime,
                    (booking, customer) => new
                    {
                        booking.DateTime,
                        booking.NewBookingCount,
                        NewCustomerCount = customer.Select(b=>b.NewCustomerCount).SingleOrDefault()
                    })
                    .ToList();

              
                // Perform a right outer join
                var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime,
                    (customer, bookings) => new
                    {
                        customer.DateTime,
                        NewBookingCount = bookings.Select(b => b.NewBookingCount).SingleOrDefault(),
                        customer.NewCustomerCount
                    })
                    .Where(x => x.NewBookingCount == 0).ToList();

                // Combine the left and right joins
                var mergedData = leftJoin.Union(rightJoin).OrderBy(data => data.DateTime).ToList();

                // Separate the counts into individual lists
                var newBookingData = mergedData.Select(d => d.NewBookingCount).ToList();
                var newCustomerData = mergedData.Select(d => d.NewCustomerCount).ToList();
                var categories = mergedData.Select(d => d.DateTime.Date.ToString("MM/dd/yyyy")).ToList();


                List<ChartData> chartDataList = new List<ChartData>
                {
                    new ChartData { Name = "New Memebers", Data = newCustomerData.ToArray() },
                    new ChartData { Name = "New Bookings", Data = newBookingData.ToArray() }
                };

                dashboardLineChartVM.Series = chartDataList;
                dashboardLineChartVM.Categories = categories.ToArray();

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw;
            }

            return dashboardLineChartVM;
        }
        public async Task<DashboardPieChartVM> GetBookingPieChartDataAsync()
        {
            DashboardPieChartVM dashboardPieChartVM = new();
            try
            {
                var totalBooking = _db.Bookings.Where(u => u.Status != SD.StatusPending && u.BookingDate >= DateTime.Now.AddDays(-30));
                var customerWithOneBooking = totalBooking.GroupBy(b => b.UserId).Where(g => g.Count() == 1).Select(g => g.Key).ToList();

                var bookingsByNewCustomer = totalBooking.Where(u => customerWithOneBooking.Any(x => x == u.UserId)).Count();
                var bookingsByReturningCustomer = totalBooking.Count() - bookingsByNewCustomer;

                dashboardPieChartVM.Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" };
                dashboardPieChartVM.Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer };

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw;
            }

            return dashboardPieChartVM;
        }
    }
}
