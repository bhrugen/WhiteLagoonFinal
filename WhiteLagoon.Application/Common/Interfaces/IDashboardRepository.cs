using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Domain.SharedModels;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IDashboardRepository 
    {
        Task<RadialBarChartVM> GetBookingsChartDataAsync();
        Task<RadialBarChartVM> GetRevenueChartDataAsync();
        Task<RadialBarChartVM> GetRegisteredUserChartDataAsync();
        Task<DashboardLineChartVM> GetMemberAndBookingChartDataAsync();
    }
}
