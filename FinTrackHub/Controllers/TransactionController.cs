using AutoMapper;
using FinTrackHub.Common;
using FinTrackHub.Interfaces;
using FinTrackHub.Models.DTOs;
using FinTrackHub.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FinTrackHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : BaseController
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        public TransactionController(ITransactionRepository transactionRepository, IMapper mapper, ICurrentUserService currentUserService)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransaction(long type)
        {
            var transaction = await _transactionRepository.GetAllTransactionAsync(UserId: null);

            if (!transaction.IsSuccess)
            {
                return HandleResult(Result<IEnumerable<TransactionDto>>.Failure(transaction.Message));
            }

            var filteredTransactions = transaction.Data
                .Where(x => x.IncomeExpenseCategory?.TypeId == type)
                .Select(x => new TransactionDto
                {
                    EncodedID = $"100 000 {x.TransactionId}",
                    TransactionId = x.TransactionId,
                    CategoryName = x.IncomeExpenseCategory?.CategoryName,
                    AccountName = x.Account?.AccountName,
                    Amount = x.Amount,
                    TransactionDate = x.TransactionDate,
                    CreatedBy = x.CreatedByUser?.UserName,
                    UpdatedBy = x.UpdatedByUser?.UserName,
                    UpdatedDate = x.UpdatedDate,
                    CreatedDate = x.CreatedDate,
                    Note = x.Note
                })
                .ToList();

            var result = Result<IEnumerable<TransactionDto>>.Success(filteredTransactions, transaction.Message);
            return HandleResult(result);
        }


        //[HttpPost]
        //public async Task<IActionResult> AddEditTransaction([FromBody] TransactionDto model)
        //{
        //    if (model == null || !ModelState.IsValid)
        //        return BadRequestResponse(GetModelErrors());

        //    var entity = _mapper.Map<Entities.Transaction>(model);
        //    entity.UpdatedById = _currentUserService.UserId; // you may have a method to get this
        //    entity.CreatedById = _currentUserService.UserId;

        //    var result = model.TransactionId == null || model.TransactionId == 0
        //        ? await _transactionRepository.AddTransactionAsync(entity)
        //        : await _transactionRepository.UpdateTransactionAsync(entity);

        //    return HandleResult(result);
        //}

        [HttpPost("add-edit")]
        public async Task<IActionResult> AddEditTransaction([FromBody] TransactionDto model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequestResponse(GetModelErrors());

            try
            {
                // Map to entity and set user context
                var entity = _mapper.Map<Entities.Transaction>(model);
                entity.UpdatedById = entity.CreatedById = _currentUserService.UserId;

                // Save or update
                var result = (model.TransactionId == null || model.TransactionId == 0)
                    ? await _transactionRepository.AddTransactionAsync(entity)
                    : await _transactionRepository.UpdateTransactionAsync(entity);

                // If save failed, return errors
                if (!result.IsSuccess)
                      return HandleResult(Result<IEnumerable<TransactionDto>>.Failure(result.Message));

                // Map back to DTO and return success response
                var dto = _mapper.Map<TransactionDto>(result.Data);
                var response = Result<TransactionDto>.Success(dto, result.Message);

                return HandleResult(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AddEditTransaction failed: {ex}");
                return StatusCode(500, "An unexpected error occurred while saving the transaction.");
            }
        }


        [HttpPost("incomeworking")]
        public async Task<IActionResult> AddEditTransactionworking([FromBody] TransactionDto model)
        {
            var info = new TransactionDto();


            if (model == null || !ModelState.IsValid)
                return BadRequestResponse(GetModelErrors());

            try
            {
                var entity = _mapper.Map<Entities.Transaction>(model);
                entity.UpdatedById = _currentUserService.UserId;
                entity.CreatedById = _currentUserService.UserId;

                var result = model.TransactionId == null || model.TransactionId == 0
                    ? await _transactionRepository.AddTransactionAsync(entity)
                    : await _transactionRepository.UpdateTransactionAsync(entity);

                if (result.IsSuccess)
                {
                    info = _mapper.Map<TransactionDto>(result.Data);
                    var results = Result<TransactionDto>.Success(info, result.Message);
                    return HandleResult(results);
                }
                return BadRequestResponse(GetModelErrors());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AddEditTransaction failed: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while saving the transaction.");
            }
        }


        [HttpPost("incomer")]
        public IActionResult AddEditTransactiontest([FromBody] object model)
        {
            // Just log to confirm it's hitting the endpoint
            Console.WriteLine("Received dummy transaction request.");

            // Return success without doing anything
            return Ok(new { message = "Transaction saved successfully (dummy response)." });
        }



        #region Transfer

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferAsync([FromBody] TransferDto model)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result<bool>.Failure("User not authenticated."));
            }

            var result = await _transactionRepository.TransferFundsAsync(
                model.sourceAccountId,
                model.destinationAccountId,
                model.Amount,
                userId);

            if (result.IsSuccess)
            {
                return HandleResult(result); // returns Result<bool> with success and message
            }

            return HandleResult(result); // returns Result<bool> with error message
        }


        #endregion



        private string GetModelErrors()
        {
            return string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }
    }
}
