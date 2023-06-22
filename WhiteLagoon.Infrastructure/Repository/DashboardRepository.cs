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

            dashboardRadialBarChartVM.TotalCount = totalBooking;
            dashboardRadialBarChartVM.IncreaseDecreaseRatio = increaseDecreaseRatio;
            dashboardRadialBarChartVM.HasRatioIncreased = hasIncreased;
            dashboardRadialBarChartVM.Series = new decimal[] { increaseDecreaseRatio };
            return dashboardRadialBarChartVM;
        }
    }
}
