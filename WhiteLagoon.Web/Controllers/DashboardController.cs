using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.SharedModels;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingsChartData()
        {
            RadialBarChartVM dashboardRadialBarChartVM = await _unitOfWork.Dashboard.GetBookingsChartDataAsync();
            return Json(dashboardRadialBarChartVM);
        }
        public async Task<IActionResult> GetTotalRevenueChartData()
        {
            RadialBarChartVM dashboardRadialBarChartVM = await _unitOfWork.Dashboard.GetRevenueChartDataAsync();
            return Json(dashboardRadialBarChartVM);
        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            RadialBarChartVM dashboardRadialBarChartVM = await _unitOfWork.Dashboard.GetRegisteredUserChartDataAsync();
            return Json(dashboardRadialBarChartVM);
        }

        public async Task<IActionResult> GetMemberAndBookingChartData()
        {
            DashboardLineChartVM dashboardLineChartVM = await _unitOfWork.Dashboard.GetMemberAndBookingChartDataAsync();

            return Json(dashboardLineChartVM);
        }

        public async Task<IActionResult> GetCustomerBookingsPieChartData()
        {
            DashboardPieChartVM dashboardPieChartVM = await _unitOfWork.Dashboard.GetBookingPieChartDataAsync();

            return Json(dashboardPieChartVM);
        }
    }
}
