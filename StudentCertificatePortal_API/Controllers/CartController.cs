using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.Controllers
{
    public class CartController : ApiControllerBase
    {
        private readonly ICartService _service;
        private readonly IMapper _mapper;
        public CartController(ICartService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<CartDto>>>> GetAllCart()
        {
            var result = await _service.GetAll(new CancellationToken());
            return Ok(Result<List<CartDto>>.Succeed(result));
        }
        [HttpGet("{userId:int}")]
        public async Task<ActionResult<Result<CartDto>>> GetCartById([FromRoute] int userId)
        {
            var result = await _service.GetCartByIdAsync(userId, new CancellationToken());
            return Ok(Result<CartDto>.Succeed(result));
        }
        [HttpPost]
        public async Task<ActionResult<Result<CartDto>>> CreateCart([FromBody] CreateCartRequest request)
        {
            var result = await _service.CreateCartAsync(request, new CancellationToken());
            return Ok(Result<CartDto>.Succeed(result));
        }
        [HttpPut("{userId:int}")]
        public async Task<ActionResult<Result<CartDto>>> UpdateCart(int userId, UpdateCartRequest request)
        {
            var result = await _service.UpdateCartAsync(userId, request, new CancellationToken());
            return Ok(Result<CartDto>.Succeed(result));
        }
        [HttpDelete("{userId:int}")]
        public async Task<ActionResult<Result<CartDto>>> DeleteCartById([FromRoute] int userId)
        {
            var result = await _service.DeleteCartAsync(userId, new CancellationToken());
            return Ok(Result<CartDto>.Succeed(result));
        }
        [HttpPost("create-carts-for-all-users")]
        public async Task<ActionResult<Result<List<CartDto>>>> CreateCartsForAllUsersWithoutCarts()
        {
            var result = await _service.CreateCartsForAllUsersWithoutCartsAsync(new CancellationToken());
            return Ok(Result<List<CartDto>>.Succeed(result));
        }
    }
}
