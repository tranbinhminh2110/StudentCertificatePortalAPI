using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Utils;
using System.Text.Json;
using System.Text;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Controllers
{
    public class TransactionController : ApiControllerBase
    {
        private readonly ITransactionService _service;
        public TransactionController(ITransactionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<TransactionDto>>>> GetAllTransaction()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<TransactionDto>>.Succeed(result));
        }

        [HttpGet("{transId:int}")]
        public async Task<ActionResult<Result<TransactionDto>>> GetTransactionById([FromRoute] int transId)
        {
            var result = await _service.GetTransactionByIdAsync(transId, new CancellationToken());
            return Ok(Result<TransactionDto>.Succeed(result));
        }

        [HttpPost]
        public async Task<ActionResult<Result<TransactionDto>>> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            var result = await _service.CreateTransactionAsync(request, new CancellationToken());
            return Ok(Result<TransactionDto>.Succeed(result));
        }

        [HttpPut("{transId:int}")]
        public async Task<ActionResult<Result<TransactionDto>>> UpdateTransaction(int transId, UpdateTransactionRequest request)
        {
            var result = await _service.UpdateTransactionAsync(transId, request, new CancellationToken());
            return Ok(Result<TransactionDto>.Succeed(result));
        }

        [HttpDelete("{transId:int}")]
        public async Task<ActionResult<Result<TransactionDto>>> DeleteTransactionById([FromRoute] int transId)
        {
            var result = await _service.DeleteTransactionAsync(transId, new CancellationToken());
            return Ok(Result<TransactionDto>.Succeed(result));
        }
        [HttpGet("/get-by-user/{userId:int}")]
        public async Task<ActionResult<Result<List<TransactionDto>>>> GetTransactionByUserId([FromRoute] int userId)
        {
            var result = await _service.GetTransactionByUserId(userId, new CancellationToken());
            return Ok(Result<List<TransactionDto>>.Succeed(result));
        }
    }
}
