using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Domain.SharedModels;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class DashboardRepository : IDashboardRepository
    {
        private ApplicationDbContext _db;
        public DashboardRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RadialBarChartVM> GetBookingsChartDataAsync()
        {
            RadialBarChartVM dashboardRadialBarChartVM = new();
            int totalBooking = await _db.Bookings.CountAsync();

            DateTime previousMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
            DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

            var countByCurrentMonth = _db.Bookings.Count(r => r.BookingDate >= currentMonthStartDate && r.BookingDate < DateTime.Now);
            var countByPreviousMonth = _db.Bookings.Count(r => r.BookingDate >= previousMonthStartDate && r.BookingDate < currentMonthStartDate);

            decimal increaseDecreaseRatio = 100;
            bool hasIncreased = true;

            // Considering any non-zero count in current month as 100% increase.
            if (countByPreviousMonth != 0)
            {
                increaseDecreaseRatio = Math.Round(((decimal)countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100, 2);
                hasIncreased = countByCurrentMonth > countByPreviousMonth;
            }
            dashboardRadialBarChartVM.IncreaseDecreaseAmount = (decimal)countByCurrentMonth;
            dashboardRadialBarChartVM.TotalCount = totalBooking;
            dashboardRadialBarChartVM.IncreaseDecreaseRatio = increaseDecreaseRatio;
            dashboardRadialBarChartVM.HasRatioIncreased = hasIncreased;
            dashboardRadialBarChartVM.Series = new decimal[] { increaseDecreaseRatio };
            return dashboardRadialBarChartVM;
        }

        public async Task<RadialBarChartVM> GetRevenueChartDataAsync()
        {
            RadialBarChartVM dashboardRadialBarChartVM = new ();
                decimal totalCost = Convert.ToDecimal(await _db.Bookings.SumAsync(x => x.TotalCost));

                
                DateTime previousMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
                DateTime currentMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var sumByCurrentMonth = _db.Bookings.Where((r => r.BookingDate >= currentMonthStartDate && r.BookingDate < DateTime.Now)).Sum(x => x.TotalCost);
                var sumByPreviousMonth = _db.Bookings.Where(r => r.BookingDate >= previousMonthStartDate && r.BookingDate < currentMonthStartDate).Sum(x => x.TotalCost);

                decimal increaseDecreaseRatio = 100;
                bool isIncrease = true;
                // Considering any non-zero count in current month as 100% increase.

                if (sumByPreviousMonth != 0)
                {
                    increaseDecreaseRatio = Convert.ToDecimal(Math.Round(((double)sumByCurrentMonth - sumByPreviousMonth) / sumByPreviousMonth * 100, 2));
                    isIncrease = sumByCurrentMonth > sumByPreviousMonth;
                }

                dashboardRadialBarChartVM.TotalCount = totalCost;
                dashboardRadialBarChartVM.IncreaseDecreaseAmount = (decimal)sumByCurrentMonth;
                dashboardRadialBarChartVM.IncreaseDecreaseRatio = increaseDecreaseRatio;
                dashboardRadialBarChartVM.HasRatioIncreased = isIncrease;
                dashboardRadialBarChartVM.Series = new decimal[] { increaseDecreaseRatio };
            return dashboardRadialBarChartVM;
        }

        public async Task<RadialBarChartVM> GetRegisteredUserChartDataAsync()
        {
            RadialBarChartVM dashboardRadialBarChartVM =    new();
                int totalCount = await _db.Users.CountAsync();

               
                DateTime previousMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
                DateTime currentMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var countByCurrentMonth = _db.Users.Count(r => r.CreatedAt >= currentMonthStartDate && r.CreatedAt < DateTime.Now);
                var countByPreviousMonth = _db.Users.Count(r => r.CreatedAt >= previousMonthStartDate && r.CreatedAt < currentMonthStartDate);

                decimal increaseDecreaseRatio = 100;
                bool isIncrease = true;
                // Considering any non-zero count in current month as 100% increase.

                if (countByPreviousMonth != 0)
                {
                    increaseDecreaseRatio = Math.Round(((decimal)countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100, 2);
                    isIncrease = countByCurrentMonth > countByPreviousMonth;
                }
                dashboardRadialBarChartVM.IncreaseDecreaseAmount = (decimal)countByCurrentMonth;
                dashboardRadialBarChartVM.TotalCount = totalCount;
                dashboardRadialBarChartVM.IncreaseDecreaseRatio = increaseDecreaseRatio;
                dashboardRadialBarChartVM.HasRatioIncreased = isIncrease;
                dashboardRadialBarChartVM.Series = new decimal[] { increaseDecreaseRatio };

            return dashboardRadialBarChartVM;
        }

    }
}
