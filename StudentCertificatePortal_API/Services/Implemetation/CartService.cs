using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public CartService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<CartDto> CreateCartAsync(CreateCartRequest request, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new Exception("User not found. Cart creation requires a valid UserId.");
            }

            var cartEntity = new Cart()
            {
                TotalPrice = 0, 
                UserId = request.UserId
            };

            if (request.ExamId != null && request.ExamId.Any())
            {
                foreach (var examId in request.ExamId)
                {
                    var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(
                        x => x.ExamId == examId, cancellationToken);
                    if (exam != null)
                    {
                        cartEntity.Exams.Add(exam);

                        var examPrice = exam.ExamDiscountFee ?? exam.ExamFee;
                        cartEntity.TotalPrice += examPrice;
                    }
                    else
                    {
                        throw new KeyNotFoundException($"SimulationExam with ID {examId} not found.");
                    }
                }
            }

            await _uow.CartRepository.AddAsync(cartEntity);

            try
            {
                await _uow.Commit(cancellationToken);

                var cartDto = _mapper.Map<CartDto>(cartEntity);

                cartDto.ExamId = cartEntity.Exams
                    .Select(exam => exam.ExamId)
                    .ToList();

                return cartDto;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"An error occurred while saving the entity changes: {innerExceptionMessage}", ex);
            }
        }

        public async Task<CartDto> DeleteCartAsync(int userId, CancellationToken cancellationToken)
        {
            var cart = await _uow.CartRepository.FirstOrDefaultAsync(
        x => x.UserId == userId, cancellationToken, include: q => q.Include(c => c.Exams));

            if (cart is null)
            {
                throw new KeyNotFoundException("Cart not found.");
            }

            cart.Exams?.Clear();

            _uow.CartRepository.Delete(cart);
            await _uow.Commit(cancellationToken);

            var cartDto = _mapper.Map<CartDto>(cart);
            return cartDto;
        }

        public async Task<List<CartDto>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _uow.CartRepository.GetAllAsync(query => query.Include(c => c.Exams));

            var cartDtos = result.Select(cart =>
            {
                var cartDto = _mapper.Map<CartDto>(cart);
                cartDto.ExamId = cart.Exams.Select(x => x.ExamId).ToList();
                return cartDto;
            }).ToList();

            return cartDtos;
        }

        public async Task<CartDto> GetCartByIdAsync(int userId, CancellationToken cancellationToken)
        {
            var cart = await _uow.CartRepository.FirstOrDefaultAsync(
    x => x.UserId == userId, cancellationToken: cancellationToken, include: query => query.Include(c => c.Exams));

            if (cart is null)
            {
                throw new KeyNotFoundException("Cart not found.");
            }

            var cartDto = _mapper.Map<CartDto>(cart);
            cartDto.ExamId = cart.Exams.Select(x => x.ExamId).ToList();
            return cartDto;
        }

        public async Task<CartDto> UpdateCartAsync(int userId, UpdateCartRequest request, CancellationToken cancellationToken)
        {
            var cart = await _uow.CartRepository
                                  .Include(x => x.Exams)
                                  .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (cart == null)
            {
                throw new KeyNotFoundException("Cart not found.");
            }

            cart.TotalPrice = 0;

            var existingExamIds = cart.Exams.Select(e => e.ExamId).ToList();
            var newExamIds = request.ExamId ?? new List<int>();

            foreach (var existingExamId in existingExamIds)
            {
                if (!newExamIds.Contains(existingExamId))
                {
                    var examToRemove = cart.Exams.FirstOrDefault(e => e.ExamId == existingExamId);
                    if (examToRemove != null)
                    {
                        cart.Exams.Remove(examToRemove);
                    }
                }
            }

            foreach (var newExamId in newExamIds)
            {
                if (!existingExamIds.Contains(newExamId))
                {
                    var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(
                        x => x.ExamId == newExamId, cancellationToken);

                    if (exam != null)
                    {
                        cart.Exams.Add(exam);

                        var examPrice = exam.ExamDiscountFee ?? exam.ExamFee;
                        cart.TotalPrice += examPrice;
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Exam with ID {newExamId} not found.");
                    }
                }
            }

            _uow.CartRepository.Update(cart);
            await _uow.Commit(cancellationToken);

            var cartDto = _mapper.Map<CartDto>(cart);
            cartDto.ExamId = cart.Exams.Select(e => e.ExamId).ToList();

            return cartDto;
        }
    }
}
