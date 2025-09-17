using AutoMapper;
using FinTrackHub.Interfaces;
using FinTrackHub.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinTrackHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : BaseController
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICurrentUserService _currentUserService;
        public DashboardController(ITransactionRepository transactionRepository, ICurrentUserService currentUserService)
        {
            _transactionRepository = transactionRepository;
            _currentUserService = currentUserService;
        }
        [HttpGet]

        public async Task<IActionResult> Get()
        {
            decimal income = 0;
            decimal expense = 0;
            var info = await _transactionRepository.GetAllTransactionAsync(UserId: null);
            income = info.Data.Where(x => x.IncomeExpenseCategory != null && x.IncomeExpenseCategory.TypeId == 3).Sum(x => x.Amount);
            expense = info.Data.Where(x => x.IncomeExpenseCategory != null && x.IncomeExpenseCategory.TypeId == 5).Sum(x => x.Amount);

            var dashboard = new
            {
                totalIncome = income,
                totalExpense = expense
            };

            var result = FinTrackHub.Common.Result<object>.Success(dashboard, "Dashboard data retrieved successfully.");
            return HandleResult(result);

        }

    }
}
