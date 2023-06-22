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

            var data = new
            {
                series = dashboardRadialBarChartVM.Series, //new int[] { 30 },
                totalCount = dashboardRadialBarChartVM.TotalCount,
                increaseDecreaseRatio = dashboardRadialBarChartVM.IncreaseDecreaseRatio,
                hasRatioIncreased = dashboardRadialBarChartVM.HasRatioIncreased,
                increaseDecreaseAmount = dashboardRadialBarChartVM.IncreaseDecreaseAmount
            };
            return Json(data);
        }
        public async Task<IActionResult> GetTotalRevenueChartData()
        {
            RadialBarChartVM dashboardRadialBarChartVM = await _unitOfWork.Dashboard.GetRevenueChartDataAsync();

            var data = new
            {
                series = dashboardRadialBarChartVM.Series, //new int[] { 30 },
                totalCount = dashboardRadialBarChartVM.TotalCount,
                increaseDecreaseRatio = dashboardRadialBarChartVM.IncreaseDecreaseRatio,
                hasRatioIncreased = dashboardRadialBarChartVM.HasRatioIncreased,
                increaseDecreaseAmount = dashboardRadialBarChartVM.IncreaseDecreaseAmount
            };
            return Json(data);
        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            RadialBarChartVM dashboardRadialBarChartVM = await _unitOfWork.Dashboard.GetRegisteredUserChartDataAsync();

            var data = new
            {
                series = dashboardRadialBarChartVM.Series, //new int[] { 30 },
                totalCount = dashboardRadialBarChartVM.TotalCount,
                increaseDecreaseRatio = dashboardRadialBarChartVM.IncreaseDecreaseRatio,
                hasRatioIncreased = dashboardRadialBarChartVM.HasRatioIncreased,
                increaseDecreaseAmount = dashboardRadialBarChartVM.IncreaseDecreaseAmount
            };
            return Json(data);
        }

        public async Task<IActionResult> GetMemberAndBookingChartData()
        {
            DashboardLineChartVM dashboardLineChartVM = await _unitOfWork.Dashboard.GetMemberAndBookingChartDataAsync();

            // Retrieve your data and format it as needed
            var data = new
            {
                series = dashboardLineChartVM.ChartData,
                categories = dashboardLineChartVM.Categories
            };

            // Manually serialize the data to JSON
            return Json(data);
        }

        public async Task<IActionResult> GetCustomerBookingsPieChartData()
        {
            DashboardPieChartVM dashboardPieChartVM = await _unitOfWork.Dashboard.GetBookingPieChartDataAsync();

            // Retrieve your data and format it as needed
            var data = new
            {
                series = dashboardPieChartVM.Series,
                labels = dashboardPieChartVM.Labels
            };

            // Manually serialize the data to JSON
            return Json(data);
        }
    }
}
