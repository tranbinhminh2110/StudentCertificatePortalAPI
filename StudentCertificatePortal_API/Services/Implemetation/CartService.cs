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
            var existingCart = await _uow.CartRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (existingCart != null)
            {
                throw new InvalidOperationException($"Cart already exists for user with ID {request.UserId}.");
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
            if (request.CourseId != null && request.CourseId.Any())
            {
                foreach (var courseId in request.CourseId)
                {
                    var course = await _uow.CourseRepository.FirstOrDefaultAsync(
                        x => x.CourseId == courseId, cancellationToken);
                    if (course != null)
                    {
                        cartEntity.Courses.Add(course);

                        var coursePrice = course.CourseDiscountFee ?? course.CourseFee;
                        cartEntity.TotalPrice += coursePrice;
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Course with ID {courseId} not found.");
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
                cartDto.CourseId = cartEntity.Courses
                    .Select(course => course.CourseId)
                    .ToList();

                return cartDto;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"An error occurred while saving the entity changes: {innerExceptionMessage}", ex);
            }
        }
        public async Task<List<CartDto>> CreateCartsForAllUsersWithoutCartsAsync(CancellationToken cancellationToken)
        {
            var carts = new List<CartDto>();
            var users = await _uow.UserRepository.GetAll();

            foreach (var user in users)
            {
                var existingCart = await _uow.CartRepository.FirstOrDefaultAsync(x => x.UserId == user.UserId, cancellationToken);
                if (existingCart == null)
                {
                    var cartEntity = new Cart()
                    {
                        TotalPrice = 0,
                        UserId = user.UserId
                    };

                    await _uow.CartRepository.AddAsync(cartEntity); 
                    await _uow.Commit(cancellationToken); 

                    carts.Add(new CartDto
                    {
                        CartId = cartEntity.CartId, 
                        TotalPrice = cartEntity.TotalPrice,
                        UserId = cartEntity.UserId,
                    });
                }
            }

            return carts; 
        }


        public async Task<CartDto> DeleteCartAsync(int userId, CancellationToken cancellationToken)
        {
            var cart = await _uow.CartRepository.FirstOrDefaultAsync(
        x => x.UserId == userId, cancellationToken, include: q => q.Include(c => c.Exams)
                                                                   .Include(c => c.Courses));

            if (cart is null)
            {
                throw new KeyNotFoundException("Cart not found.");
            }

            cart.Exams?.Clear();
            cart.Courses?.Clear();

            _uow.CartRepository.Delete(cart);
            await _uow.Commit(cancellationToken);

            var cartDto = _mapper.Map<CartDto>(cart);
            return cartDto;
        }

        public async Task<List<CartDto>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _uow.CartRepository.GetAllAsync(query => query.Include(c => c.Exams)
            .Include(c => c.Courses));

            var cartDtos = result.Select(cart =>
            {
                var cartDto = _mapper.Map<CartDto>(cart);
                cartDto.ExamDetails = cart.Exams
                    .Select(exam => new ExamDetailsDto
                    {
                        ExamId = exam.ExamId,
                        ExamName = exam.ExamName,
                        ExamCode = exam.ExamCode,
                        ExamFee = exam.ExamFee,
                        ExamDiscountFee = exam.ExamDiscountFee,
                        ExamImage = exam.ExamImage,
                    }).ToList();
                cartDto.CourseDetails = cart.Courses
                    .Select(course => new CourseDetailsDto
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        CourseCode = course.CourseCode,
                        CourseTime = course.CourseTime,
                        CourseFee = course.CourseFee,
                        CourseDiscountFee = course.CourseDiscountFee,
                        CourseImage = course.CourseImage,

                    }).ToList();
                return cartDto;
            }).ToList();

            return cartDtos;
        }

        public async Task<CartDto> GetCartByIdAsync(int userId, CancellationToken cancellationToken)
        {
            var cart = await _uow.CartRepository.FirstOrDefaultAsync(
    x => x.UserId == userId, cancellationToken: cancellationToken, include: query => query.Include(c => c.Exams)
                                                                                          .Include(c => c.Courses));

            if (cart is null)
            {
                throw new KeyNotFoundException("Cart not found.");
            }

            var cartDto = _mapper.Map<CartDto>(cart);
            cartDto.ExamDetails = cart.Exams
                    .Select(exam => new ExamDetailsDto
                    {
                        ExamId = exam.ExamId,
                        ExamName = exam.ExamName,
                        ExamCode = exam.ExamCode,
                        ExamFee = exam.ExamFee,
                        ExamDiscountFee = exam.ExamDiscountFee,
                        ExamImage = exam.ExamImage,
                    }).ToList();
            cartDto.CourseDetails = cart.Courses
                .Select(course => new CourseDetailsDto
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseCode = course.CourseCode,
                    CourseTime = course.CourseTime,
                    CourseFee = course.CourseFee,
                    CourseDiscountFee = course.CourseDiscountFee,
                    CourseImage = course.CourseImage,

                }).ToList();
            return cartDto;
        }

        public async Task<CartDto> UpdateCartAsync(int userId, UpdateCartRequest request, CancellationToken cancellationToken)
        {
            var cart = await _uow.CartRepository
                                  .Include(x => x.Exams)
                                  .Include(x => x.Courses)
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
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Exam with ID {newExamId} not found.");
                    }
                }
            }

            var existingCourseIds = cart.Courses.Select(c => c.CourseId).ToList();
            var newCourseIds = request.CourseId ?? new List<int>();

            foreach (var existingCourseId in existingCourseIds)
            {
                if (!newCourseIds.Contains(existingCourseId))
                {
                    var courseToRemove = cart.Courses.FirstOrDefault(c => c.CourseId == existingCourseId);
                    if (courseToRemove != null)
                    {
                        cart.Courses.Remove(courseToRemove);
                    }
                }
            }

            foreach (var newCourseId in newCourseIds)
            {
                if (!existingCourseIds.Contains(newCourseId))
                {
                    var course = await _uow.CourseRepository.FirstOrDefaultAsync(
                        x => x.CourseId == newCourseId, cancellationToken);

                    if (course != null)
                    {
                        cart.Courses.Add(course);
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Course with ID {newCourseId} not found.");
                    }
                }
            }

            var examPrice = cart.Exams.Sum(e => e.ExamDiscountFee ?? e.ExamFee);
            var coursePrice = cart.Courses.Sum(c => c.CourseDiscountFee ?? c.CourseFee);

            if (examPrice > 0 && coursePrice > 0)
            {
                cart.TotalPrice = (int)examPrice + (int)coursePrice; 
            }
            else if (examPrice > 0) 
            {
                cart.TotalPrice = (int)examPrice;
            }
            else if (coursePrice > 0) 
            {
                cart.TotalPrice = (int)coursePrice;
            }

            _uow.CartRepository.Update(cart);
            await _uow.Commit(cancellationToken);

            var cartDto = _mapper.Map<CartDto>(cart);
            cartDto.ExamId = cart.Exams.Select(e => e.ExamId).ToList();
            cartDto.CourseId = cart.Courses.Select(c => c.CourseId).ToList();

            return cartDto;
        }


    }
}
